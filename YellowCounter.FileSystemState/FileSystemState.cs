using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using YellowCounter.FileSystemState.PathRedux;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.Options;
using YellowCounter.FileSystemState.Filter;

namespace YellowCounter.FileSystemState
{
    public class FileSystemState
    {
        private PathToFileStateHashtable _state;
        private AcceptFileSystemEntry acceptFileSystemEntry;
        private readonly PathStorage pathStorage;
        private FileSystemStateOptions options;
        private Func<IFileSystemEnumerator> newFileSystemEnumerator;

        public FileSystemState(string rootDir, string filter = "*")
            : this(rootDir, new FileSystemStateOptions()
                  .WithFilter(filter))
        {
        }

        public FileSystemState(string rootDir, string filter, FileSystemStateOptions options)
            : this(rootDir, options.WithFilter(filter))
        {
        }

        public FileSystemState(string rootDir, FileSystemStateOptions options)
        {
            this.options = (options == null)
                ? new FileSystemStateOptions()
                : options.Clone();

            this.RootDir = rootDir ?? throw new ArgumentNullException(nameof(rootDir));

            if(!Directory.Exists(rootDir))
                throw new DirectoryNotFoundException();

            this.pathStorage = new PathStorage(this.options.PathStorageOptions);

            _state = new PathToFileStateHashtable(this.pathStorage, this.options.FileStateReferenceSetOptions);

            this.acceptFileSystemEntry = new AcceptFileSystemEntry(
                _state,
                this.options.Filter,
                this.options.DirectoryFilter);

            this.newFileSystemEnumerator = () =>
            {
                return new FileSystemChangeEnumerator(
                    new FileSystemEnumeratorOptions()
                    {
                        RootDir = this.RootDir,
                        EnumerationOptions = toEnumerationOptions(this.options),
                    },
                    this.acceptFileSystemEntry);
            };
        }

        public string RootDir { get; private set; }


        public void LoadState()
        {
            // Set initial baseline by reading current directory state without returning
            // every file as a change.
            gatherChanges();
            acceptChanges();
        }


        // This function walks all watched files, collects changes, and updates state
        public IList<FileChange> GetChanges()
        {
            // Get the raw file changes, either create, file change or removal.
            var (creates, changes, removals) = getFileChanges();

            // Match up the creates and removals to get the renames
            var renames = matchRenames(creates, removals);

            // Convert to the output format.
            var result = convertToFileChanges(creates, changes, removals, renames);

            return result;
        }


        private void gatherChanges()
        {
            using(var enumerator = newFileSystemEnumerator())
            {
                // The FileSystemEnumerator doesn't act like a normal enumerator. The
                // Reset() method is not supported. Each item is presented as a ref
                // FileSystemEntry to the TransformEntry method. The result of that is
                // then available on Current. We can simply look at the FileSystemEntry
                // at TransformEntry time and ignore Current completely.
                //
                // Enumerating causes TransformEntry() to be called repeatedly
                while(enumerator.MoveNext()) { };
            }
        }

        //public void TransformEntry(in FileSystemEntry fileSystemEntry)
        //{
        //    _state.TransformEntry(in fileSystemEntry);
        //}

        //public bool ShouldIncludeEntry(ref FileSystemEntry entry)
        //{
        //    if(entry.IsDirectory)
        //        return false;

        //    return options.Filter.ShouldInclude(entry.FileName);
        //}

        //public bool ShouldRecurseIntoEntry(ref FileSystemEntry entry)
        //{
        //    return options.DirectoryFilter.ShouldInclude(entry.FileName);
        //}

        private void acceptChanges()
        {
            // Clear out the files that have been removed or renamed from our state.
            _state.Sweep();
        }

        private List<FileChange> convertToFileChanges(
            IEnumerable<FileState> creates, 
            IEnumerable<FileState> changes, 
            IEnumerable<FileState> removals, 
            IEnumerable<(FileState NewFile, FileState OldFile)> renames)
        {
            var createResults = creates
                .Except(renames.Select(x => x.NewFile))
                .Select(x => newFileChange(x.DirectoryRef, x.FilenameRef, WatcherChangeTypes.Created))
                ;

            var changeResults = changes
                .Select(x => newFileChange(x.DirectoryRef, x.FilenameRef, WatcherChangeTypes.Changed))
                ;

            var removeResults = removals
                .Except(renames.Select(x => x.OldFile))
                .Select(x => newFileChange(x.DirectoryRef, x.FilenameRef, WatcherChangeTypes.Deleted))
                ;

            var renameResults = renames.Select(x => new FileChange(
                    pathStorage.CreateString(x.NewFile.DirectoryRef),
                    pathStorage.CreateString(x.NewFile.FilenameRef),
                    WatcherChangeTypes.Renamed,
                    pathStorage.CreateString(x.OldFile.DirectoryRef),
                    pathStorage.CreateString(x.OldFile.FilenameRef)
                    ));
            
            var result = new List<FileChange>();

            result.AddRange(createResults);
            result.AddRange(changeResults);
            result.AddRange(removeResults);
            result.AddRange(renameResults);

            return result;

            FileChange newFileChange(
                int directoryRef, 
                int filenameRef,
                WatcherChangeTypes changeType)
            {
                return new FileChange(
                    pathStorage.CreateString(directoryRef),
                    pathStorage.CreateString(filenameRef),
                    changeType);
            }
        }

        private (
            IEnumerable<FileState> creates,
            IEnumerable<FileState> changes,
            IEnumerable<FileState> removals) getFileChanges()
        {
            var creates = new List<FileState>();
            var changes = new List<FileState>();
            var removals = new List<FileState>();

            gatherChanges();

            foreach(ref readonly var x in _state)
            {
                if(x.Flags.HasFlag(FileStateFlags.Seen))
                {
                    if(x.Flags.HasFlag(FileStateFlags.Created))
                        creates.Add(x);
                    else if(x.Flags.HasFlag(FileStateFlags.Changed))
                        changes.Add(x);
                }
                else if(x.Flags == FileStateFlags.None)
                {
                    removals.Add(x);
                }
            }

            acceptChanges();

            return (creates, changes, removals);
        }

        private IEnumerable<(FileState NewFile, FileState OldFile)> matchRenames(
            IEnumerable<FileState> creates,
            IEnumerable<FileState> removals)
        {
            // Want to match creates and removals to convert to renames either by:
            // Same directory, different name
            // or different directory, same name.
            return matchRenames(creates, removals, false)
                .Concat(matchRenames(creates, removals, true));
        }

        private IEnumerable<(FileState NewFile, FileState OldFile)> matchRenames(
            IEnumerable<FileState> creates,
            IEnumerable<FileState> removals,
            bool byName)
        {
            var createsByTime = creates
                .GroupBy(x => new
                {
                    // Group by last write time, length and directory or filename
                    x.Signature,
                    Name = byName ? x.DirectoryRef : x.FilenameRef
                },
                    (x, y) => new
                    {
                        // Return key fields, and list of all created files for the
                        // given (time, length, path) key
                        x.Signature,
                        x.Name,
                        Creates = y.ToList()
                    })
                .ToList();

            var removesByTime = removals
                .GroupBy(x => new { x.Signature, Name = byName ? x.DirectoryRef : x.FilenameRef },
                (x, y) => new { x.Signature, x.Name, Removes = y.ToList() })
                .ToList();

            // Join creates and removes by (time, length, directory), then filter to
            // only those matches which are unambiguous.
            return createsByTime.Join(removesByTime,
                x => new { x.Signature, x.Name },
                x => new { x.Signature, x.Name },
                (x, y) => new { x.Creates, y.Removes }
                )
                .Where(x => x.Creates.Count == 1 && x.Removes.Count == 1)
                .Select(x => (
                    NewFile: x.Creates[0],
                    OldFile: x.Removes[0]
                ))
                .ToList();
        }


        private static EnumerationOptions toEnumerationOptions(FileSystemStateOptions options)
        {
            return new EnumerationOptions()
            {
                RecurseSubdirectories = options.RecurseSubdirectories,
                IgnoreInaccessible = options.IgnoreInaccessible,
                AttributesToSkip = options.AttributesToSkip,
            };
        }

    }
}

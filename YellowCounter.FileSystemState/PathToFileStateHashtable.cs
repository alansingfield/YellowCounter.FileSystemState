using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.IO.Enumeration;
using YellowCounter.FileSystemState.PathRedux;
using System.Diagnostics;
using System.IO;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState
{
    internal class PathToFileStateHashtable : IDisposable
    {
        FileStateReferenceSet dict;
        private bool disposedValue;
        private readonly IPathStorage pathStorage;

        public PathToFileStateHashtable(IPathStorage pathStorage, FileStateReferenceSetOptions options) 
        {
            dict = new FileStateReferenceSet(options);

            this.pathStorage = pathStorage;
        }

        internal void Mark(in FileSystemEntry input)
        {
            // Look up the directory string and filename string, convert to a reference
            // number in pathStorage.
            int dirRef = pathStorage.Store(input.Directory);
            int filenameRef = pathStorage.Store(input.FileName);

            // Find the directory/filename in the dictionary
            int index = dict.IndexOf((dirRef, filenameRef));
            if(index != -1)
            {
                // In-place update of the found element
                ref var fileState = ref dict.ElementAt(index);

                markExisting(ref fileState, in input);
            }
            else
            {
                // Not found? Create a new one and add it in
                var fileState = newFileState(in input, dirRef, filenameRef);

                dict.Add((dirRef, filenameRef), fileState);
            }
        }

        private static FileState newFileState(in FileSystemEntry input, int dirRef, int filenameRef)
        {
            FileState fileState;

            // It's a new one so mark as "Created". Also set the Seen flag.
            fileState.Flags = FileStateFlags.Created | FileStateFlags.Seen;

            fileState.DirectoryRef =        dirRef;
            fileState.FilenameRef =         filenameRef;

            fileState.Signature = makeSignature(input.LastWriteTimeUtc, input.Length);

            return fileState;
        }

        private static int makeSignature(DateTimeOffset lastWriteTimeUtc, long length)
        {
            unchecked
            {
                // Combine datetime and length into 32 bit hash.
                ulong uticks = (ulong)lastWriteTimeUtc.UtcTicks;
                ulong ulength = (ulong)length;
            
                return
                      (int)(uticks & 0xFFFFFFFF) 
                    ^ (int)(uticks >> 32)
                    ^ (int)(ulength & 0xFFFFFFFF)
                    ^ (int)(ulength >> 32)
                ;
            }
        }

        private void markExisting(ref FileState fs, in FileSystemEntry input)
        {
            // Mark that we've seen the file.
            fs.Flags |= FileStateFlags.Seen;

            int signature = makeSignature(input.LastWriteTimeUtc, input.Length);

            // Has it changed since we last saw it?
            if(fs.Signature != signature)
            {
                fs.Flags |= FileStateFlags.Changed;

                // Update the last write time / file length.
                fs.Signature = signature;
            }
        }

        public HashBucket<FileState>.Enumerator GetEnumerator() => dict.GetEnumerator();


        public void Sweep()
        {
            // Use the index-based enumeration because we need to delete, deletion
            // needs the index.
            foreach(int idx in dict.AllIndices())
            {
                ref var fileState = ref dict.ElementAt(idx);

                // All elements that have not been seen on the last sweep are now
                // no longer any use to us and can be deleted.
                if(!fileState.Flags.HasFlag(FileStateFlags.Seen))
                {
                    dict.DeleteAt(idx);
                }
                else
                {
                    // Clear the flags for next time.
                    fileState.Flags = FileStateFlags.None;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    dict?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}

using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;
using YellowCounter.FileSystemState.Filter;

namespace YellowCounter.FileSystemState
{
    internal class AcceptFileSystemEntry : IAcceptFileSystemEntry
    {
        private readonly IFileStateStorage fileStateStorage;
        private readonly IFilenameFilter filenameFilter;
        private readonly IDirectoryFilter directoryFilter;

        public AcceptFileSystemEntry(
            IFileStateStorage fileStateStorage, 
            IFilenameFilter filenameFilter,
            IDirectoryFilter directoryFilter)
        {
            this.fileStateStorage = fileStateStorage;
            this.filenameFilter = filenameFilter;
            this.directoryFilter = directoryFilter;
        }

        public void TransformEntry(in FileSystemEntry fileSystemEntry)
        {
            fileStateStorage.Mark(in fileSystemEntry);
        }

        public bool ShouldIncludeEntry(in FileSystemEntry entry)
        {
            if(entry.IsDirectory)
                return false;

            return filenameFilter.ShouldInclude(entry.FileName);
        }

        public bool ShouldRecurseIntoEntry(in FileSystemEntry entry)
        {
            return directoryFilter.ShouldInclude(entry.FileName);
        }
    }
}

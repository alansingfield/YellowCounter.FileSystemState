using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;
using YellowCounter.FileSystemState.Filter;

namespace YellowCounter.FileSystemState
{
    internal class AcceptFileSystemEntry : IAcceptFileSystemEntry
    {
        private readonly IFileStateStorage fileStateHashTable;
        private readonly IFilenameFilter filenameFilter;
        private readonly IDirectoryFilter directoryFilter;

        public AcceptFileSystemEntry(
            IFileStateStorage pathToFileStateHashtable, 
            IFilenameFilter filenameFilter,
            IDirectoryFilter directoryFilter)
        {
            this.fileStateHashTable = pathToFileStateHashtable;
            this.filenameFilter = filenameFilter;
            this.directoryFilter = directoryFilter;
        }

        public void TransformEntry(in FileSystemEntry fileSystemEntry)
        {
            fileStateHashTable.Mark(in fileSystemEntry);
        }

        public bool ShouldIncludeEntry(ref FileSystemEntry entry)
        {
            if(entry.IsDirectory)
                return false;

            return filenameFilter.ShouldInclude(entry.FileName);
        }

        public bool ShouldRecurseIntoEntry(ref FileSystemEntry entry)
        {
            return directoryFilter.ShouldInclude(entry.FileName);
        }
    }
}

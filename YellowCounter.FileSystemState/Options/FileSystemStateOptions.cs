using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YellowCounter.FileSystemState.Filter;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.PathRedux;

namespace YellowCounter.FileSystemState.Options
{
    public class FileSystemStateOptions
    {
        public bool RecurseSubdirectories { get; set; }
        public bool IgnoreInaccessible { get; set; } = true;
        public FileAttributes AttributesToSkip { get; set; } = FileAttributes.Hidden | FileAttributes.System;

        public IFilenameFilter Filter { get; set; }
        public IDirectoryFilter DirectoryFilter { get; set; }
        public PathStorageOptions PathStorageOptions { get; set; }
    }
}

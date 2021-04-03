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
        public bool RecurseSubdirectories { get; set; } = false;
        public bool IgnoreInaccessible { get; set; } = true;
        public FileAttributes AttributesToSkip { get; set; } = FileAttributes.Hidden | FileAttributes.System;

        /// <summary>
        /// Filter for filenames
        /// </summary>
        public IFilenameFilter Filter { get; set; } = new FilenameFilter();
        /// <summary>
        /// Filter for directory names. Filter is not tested on the root folder.
        /// </summary>
        public IDirectoryFilter DirectoryFilter { get; set; } = new DirectoryFilter();
        /// <summary>
        /// Options for the storage of path strings.
        /// </summary>
        public PathStorageOptions PathStorageOptions { get; set; } = new PathStorageOptions();

        public FileStateReferenceSetOptions FileStateReferenceSetOptions { get; set; } = new FileStateReferenceSetOptions();

        public FileSystemStateOptions Clone() => (FileSystemStateOptions)this.MemberwiseClone();
    }
}

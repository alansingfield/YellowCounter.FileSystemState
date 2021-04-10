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
        public virtual bool RecurseSubdirectories { get; set; } = false;
        public virtual bool IgnoreInaccessible { get; set; } = true;
        public virtual FileAttributes AttributesToSkip { get; set; } = FileAttributes.Hidden | FileAttributes.System;

        /// <summary>
        /// Filter for filenames
        /// </summary>
        public virtual IFilenameFilter Filter { get; set; } = new FilenameFilter();
        /// <summary>
        /// Filter for directory names. Filter is not tested on the root folder.
        /// </summary>
        public virtual IDirectoryFilter DirectoryFilter { get; set; } = new DirectoryFilter();

        public virtual FileSystemStateOptions Clone() => (FileSystemStateOptions)this.MemberwiseClone();
    }
}

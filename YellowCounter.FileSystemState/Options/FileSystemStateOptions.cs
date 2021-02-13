using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YellowCounter.FileSystemState.PathRedux;

namespace YellowCounter.FileSystemState.Options
{
    public class FileSystemStateOptions : EnumerationOptions
    {
        public string RootDir { get; set; }
        public string Filter { get; set; }
        public PathStorageOptions PathStorageOptions { get; set; }
    }
}

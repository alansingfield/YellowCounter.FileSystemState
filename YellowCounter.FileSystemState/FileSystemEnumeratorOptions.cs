using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YellowCounter.FileSystemState
{
    public class FileSystemEnumeratorOptions
    {
        public string RootDir { get; set; }
        public EnumerationOptions EnumerationOptions { get; set; }
    }
}

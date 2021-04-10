using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState
{
    public interface IRootDir
    {
        string Folder { get; set; }
    }

    public class RootDir : IRootDir
    {
        public string Folder { get; set; }
    }
}

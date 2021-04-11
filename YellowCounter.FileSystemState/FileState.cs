using System;

namespace YellowCounter.FileSystemState
{
    internal struct FileState
    {
        public int DirectoryRef;
        public int FilenameRef;
        public int Signature;
        public FileStateFlags Flags;
    }

    [Flags]
    public enum FileStateFlags : byte
    {
        None = 0,
        Seen = 1,
        Created = 2,
        Changed = 4
    }
}

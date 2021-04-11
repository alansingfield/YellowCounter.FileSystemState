using System.Collections.Generic;

namespace YellowCounter.FileSystemState
{
    internal interface IFileSystemStateInternal
    {
        void Attach();
        IList<FileChange> GetChanges();
    }
}
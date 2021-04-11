using System.Collections.Generic;

namespace YellowCounter.FileSystemState
{
    public interface IFileSystemState
    {
        string RootDir { get; }

        void Attach();
        IList<FileChange> GetChanges();
    }
}
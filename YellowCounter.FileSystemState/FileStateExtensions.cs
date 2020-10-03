using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.PathRedux;

namespace YellowCounter.FileSystemState
{
    internal static class FileStateExtensions
    {
        internal static bool TryStore(this HashBucket<FileState> hashBucket, in FileState fileState)
        {
            return hashBucket.TryStore(
                HashCode.Combine(
                    fileState.DirectoryRef,
                    fileState.FilenameRef),
                fileState);
        }
    }
}

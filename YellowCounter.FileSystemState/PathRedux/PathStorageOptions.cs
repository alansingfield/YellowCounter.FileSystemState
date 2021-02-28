using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class PathStorageOptions
    {
        public HashedCharBufferOptions HashedCharBufferOptions { get; set; }
        public HashBucketOptions HashBucketOptions { get; set; }
    }
}

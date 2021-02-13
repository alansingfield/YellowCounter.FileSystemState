using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class PathStorageOptions
    {
        //public int HashBucketInitialCapacity { get; set; }
        //public int InitialCharCapacity { get; set; }
        //public int InitialHashCapacity { get; set; }

        public HashedCharBufferOptions HashedCharBufferOptions { get; set; }
        public HashBucketOptions HashBucketOptions { get; set; }
    }
}

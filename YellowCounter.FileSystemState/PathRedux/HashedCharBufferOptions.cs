using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class HashedCharBufferOptions
    {
        public Func<IHashCode> NewHashCode { get; set; }
        public int InitialCharCapacity { get; set; }
        public HashBucketOptions HashBucketOptions { get; set; }

    }
}

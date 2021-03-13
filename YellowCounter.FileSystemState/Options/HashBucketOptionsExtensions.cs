using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState.Options
{
    public static class HashBucketOptionsExtensions
    {
        internal static HashBucketOptions ApplyDefaults(this HashBucketOptions options)
        {
            if(options.Capacity <= 0)
                options.Capacity = 256;

            if(options.ChunkSize < 0)
                options.ChunkSize = 32;

            return options;
        }


        public static HashBucketOptions WithCapacity(this HashBucketOptions options, int capacity)
        {
            options.Capacity = capacity;
            return options;
        }

        public static HashBucketOptions WithChunkSize(this HashBucketOptions options, int chunkSize)
        {
            options.ChunkSize = chunkSize;
            return options;
        }
    }
}

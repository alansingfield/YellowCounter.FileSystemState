using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.PathRedux;
using YellowCounter.FileSystemState.Sizing;

namespace YellowCounter.FileSystemState.Options
{
    public static class PathStorageOptionsExtensions
    {
        public static PathStorageOptions WithHashedCharBufferOptions(this PathStorageOptions pathStorageOptions,
            HashedCharBufferOptions hashedCharBufferOptions)
        {
            pathStorageOptions.HashedCharBufferOptions = hashedCharBufferOptions;
            
            return pathStorageOptions;
        }

        public static PathStorageOptions WithHashBucketOptions(this PathStorageOptions pathStorageOptions,
            HashBucketOptions hashBucketOptions)
        {
            pathStorageOptions.HashBucketOptions = hashBucketOptions;

            return pathStorageOptions;
        }

        public static PathStorageOptions WithSizePolicy(this PathStorageOptions pathStorageOptions,
            ISizePolicy sizePolicy)
        {
            pathStorageOptions.SizePolicy = sizePolicy;

            return pathStorageOptions;
        }

    }
}

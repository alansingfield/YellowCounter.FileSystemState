using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.PathRedux;

namespace YellowCounter.FileSystemState.Options
{
    public static class PathStorageOptionsExtensions
    {
        public static PathStorageOptions ApplyDefaults(this PathStorageOptions options)
        {
            options.HashBucketOptions ??= new HashBucketOptions();
            //options.HashBucketOptions.ApplyDefaults();

            options.HashedCharBufferOptions ??= new HashedCharBufferOptions();
            options.HashedCharBufferOptions.ApplyDefaults();

            return options;
        }
    }
}

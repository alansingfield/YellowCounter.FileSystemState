using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.PathRedux;
using YellowCounter.FileSystemState.HashCodes;

namespace YellowCounter.FileSystemState.Options
{
    public static class HashedCharBufferOptionsExtensions
    {
        public static HashedCharBufferOptions ApplyDefaults(this HashedCharBufferOptions options)
        {
            if(options.InitialCharCapacity <= 0)
                options.InitialCharCapacity = 1024;

            options.NewHashCode ??= () => new StandardHashCode();

            options.HashBucketOptions ??= new HashBucketOptions();

            return options;
        }
    }
}

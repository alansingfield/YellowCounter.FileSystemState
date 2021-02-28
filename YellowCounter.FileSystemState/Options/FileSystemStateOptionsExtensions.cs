using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Filter;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.PathRedux;

namespace YellowCounter.FileSystemState.Options
{
    public static class FileSystemStateOptionsExtensions
    {
        public static FileSystemStateOptions WithFilter(this FileSystemStateOptions options, string pattern)
        {
            if(pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            options.Filter = new FilenameFilter() { Pattern = pattern };

            return options;
        }

        public static FileSystemStateOptions WithFilter(this FileSystemStateOptions options, IFilenameFilter filter)
        {
            options.Filter = filter ?? throw new ArgumentNullException(nameof(filter));

            return options;
        }

        public static FileSystemStateOptions WithRecurseSubdirectories(this FileSystemStateOptions options, bool recurseSubdirectories = true)
        {
            options.RecurseSubdirectories = recurseSubdirectories;

            return options;
        }


        internal static FileSystemStateOptions ApplyDefaults(this FileSystemStateOptions options)
        {
            options.Filter ??= new FilenameFilter();

            options.PathStorageOptions ??= new PathStorageOptions();

            options.PathStorageOptions.ApplyDefaults();

            return options;
        }

        internal static HashBucketOptions ApplyDefaults(this HashBucketOptions options)
        {
            if(options.Capacity <= 0)
                options.Capacity = 256;

            if(options.ChunkSize < 0)
                options.ChunkSize = 32;

            return options;
        }

        internal static PathStorageOptions ApplyDefaults(this PathStorageOptions options)
        {
            options.HashBucketOptions ??= new HashBucketOptions();
            options.HashBucketOptions.ApplyDefaults();

            options.HashedCharBufferOptions ??= new HashedCharBufferOptions();
            options.HashedCharBufferOptions.ApplyDefaults();

            return options;
        }

        internal static HashedCharBufferOptions ApplyDefaults(this HashedCharBufferOptions options)
        {
            if(options.InitialCharCapacity <= 0)
                options.InitialCharCapacity = 1024;

            options.NewHashCode ??= () => new StandardHashCode();

            options.HashBucketOptions ??= new HashBucketOptions();
            options.HashBucketOptions.ApplyDefaults();

            return options;
        }
    }
}

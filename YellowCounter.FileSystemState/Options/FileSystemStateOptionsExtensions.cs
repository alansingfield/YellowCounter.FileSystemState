using System;
using System.Collections.Generic;
using System.IO;
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
        public static FileSystemStateOptions WithDirectoryFilter(this FileSystemStateOptions options, string pattern)
        {
            if(pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            options.DirectoryFilter = new DirectoryFilter() { Pattern = pattern };

            return options;
        }

        public static FileSystemStateOptions WithDirectoryFilter(this FileSystemStateOptions options, IDirectoryFilter directoryFilter)
        {
            options.DirectoryFilter = directoryFilter ?? throw new ArgumentNullException(nameof(directoryFilter));

            return options;
        }

        public static FileSystemStateOptions WithRecurseSubdirectories(this FileSystemStateOptions options, bool recurseSubdirectories = true)
        {
            options.RecurseSubdirectories = recurseSubdirectories;

            return options;
        }

        public static FileSystemStateOptions WithIgnoreInaccessible(this FileSystemStateOptions options, bool ignoreInaccessible)
        {
            options.IgnoreInaccessible = ignoreInaccessible;

            return options;
        }

        public static FileSystemStateOptions WithAttributesToSkip(this FileSystemStateOptions options, FileAttributes attributesToSkip)
        {
            options.AttributesToSkip = attributesToSkip;

            return options;
        }


        public static FileSystemStateOptions WithPathStorageOptions(this FileSystemStateOptions options, PathStorageOptions pathStorageOptions)
        {
            options.PathStorageOptions = pathStorageOptions;

            return options;
        }

        public static FileSystemStateOptions WithFileStateReferenceSetOptions(this FileSystemStateOptions options, FileStateReferenceSetOptions fileStateReferenceSetOptions)
        {
            options.FileStateReferenceSetOptions = fileStateReferenceSetOptions;

            return options;
        }







        public static FileSystemStateOptions2 WithFilter(this FileSystemStateOptions2 options, string pattern)
        {
            if(pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            options.Filter = new FilenameFilter() { Pattern = pattern };

            return options;
        }

        public static FileSystemStateOptions2 WithFilter(this FileSystemStateOptions2 options, IFilenameFilter filter)
        {
            options.Filter = filter ?? throw new ArgumentNullException(nameof(filter));

            return options;
        }
        public static FileSystemStateOptions2 WithDirectoryFilter(this FileSystemStateOptions2 options, string pattern)
        {
            if(pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            options.DirectoryFilter = new DirectoryFilter() { Pattern = pattern };

            return options;
        }

        public static FileSystemStateOptions2 WithDirectoryFilter(this FileSystemStateOptions2 options, IDirectoryFilter directoryFilter)
        {
            options.DirectoryFilter = directoryFilter ?? throw new ArgumentNullException(nameof(directoryFilter));

            return options;
        }

        public static FileSystemStateOptions2 WithRecurseSubdirectories(this FileSystemStateOptions2 options, bool recurseSubdirectories = true)
        {
            options.RecurseSubdirectories = recurseSubdirectories;

            return options;
        }

        public static FileSystemStateOptions2 WithIgnoreInaccessible(this FileSystemStateOptions2 options, bool ignoreInaccessible)
        {
            options.IgnoreInaccessible = ignoreInaccessible;

            return options;
        }

        public static FileSystemStateOptions2 WithAttributesToSkip(this FileSystemStateOptions2 options, FileAttributes attributesToSkip)
        {
            options.AttributesToSkip = attributesToSkip;

            return options;
        }


        //public static FileSystemStateOptions2 WithPathStorageOptions(this FileSystemStateOptions2 options, PathStorageOptions pathStorageOptions)
        //{
        //    options.PathStorageOptions = pathStorageOptions;

        //    return options;
        //}

        //public static FileSystemStateOptions2 WithFileStateReferenceSetOptions(this FileSystemStateOptions2 options, FileStateReferenceSetOptions fileStateReferenceSetOptions)
        //{
        //    options.FileStateReferenceSetOptions = fileStateReferenceSetOptions;

        //    return options;
        //}













    }
}

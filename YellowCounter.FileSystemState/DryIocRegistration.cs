using DryIoc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YellowCounter.FileSystemState.Filter;
using YellowCounter.FileSystemState.Options;
using YellowCounter.FileSystemState.PathRedux;

namespace YellowCounter.FileSystemState
{
    internal static class DryIocRegistration
    {
        public static void RegisterFileSystemState(this IContainer container)
        {
            // Options objects use their default constructors
            container.Register<FileSystemStateOptions>(Reuse.Scoped);
            container.Register<PathStorageOptions>(Reuse.Scoped);
            container.Register<EnumerationOptions>(Reuse.Scoped);
            container.Register<FileStateReferenceSetOptions>(Reuse.Scoped);

            container.Register<IRootDir, RootDir>(Reuse.Scoped);
            container.Register<IFilenameFilter, FilenameFilter>(Reuse.Scoped);
            container.Register<IDirectoryFilter, DirectoryFilter>(Reuse.Scoped);
            container.Register<IAcceptFileSystemEntry, AcceptFileSystemEntry>(Reuse.Scoped);
            container.Register<IFileSystemEnumerator, FileSystemChangeEnumerator>(Reuse.Scoped);
            container.Register<IPathToFileStateHashtable, PathToFileStateHashtable>(Reuse.Scoped);

            container.Register<PathStorage>(Reuse.Scoped);
            container.Register<FileSystemState2>(Reuse.Scoped);
        }

    }
}

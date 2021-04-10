﻿using DryIoc;
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
            
            // We want a new instance of this one each time.
            container.Register<IFileSystemEnumerator, FileSystemChangeEnumerator>(
                Reuse.Transient,
                setup: Setup.With(allowDisposableTransient: true));

            container.Register<IFileStateStorage, FileStateStorage>(Reuse.Scoped);

            container.Register<IPathStorage, PathStorage>(Reuse.Scoped);
            container.Register<FileSystemStateInternal>(Reuse.Scoped);
        }

    }
}

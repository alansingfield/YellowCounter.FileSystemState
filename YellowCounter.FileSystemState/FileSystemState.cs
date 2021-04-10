using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DryIoc;
using YellowCounter.FileSystemState.Filter;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState
{
    public class FileSystemState
    {
        // Static section, set up the container in the default configuration.
        private static IContainer s_globalContainer;

        static FileSystemState()
        {
            s_globalContainer = new Container();
            s_globalContainer.RegisterFileSystemState();
        }


        internal IResolverContext Context { get; private set; }

        private FileSystemStateInternal fileSystemState;
        private IRootDir root;

        public FileSystemState(string rootDir, string filter = "*")
            : this(rootDir, new FileSystemStateOptions()
                  .WithFilter(filter))
        {
        }

        public FileSystemState(string rootDir, string filter, FileSystemStateOptions options)
            : this(rootDir, options.WithFilter(filter))
        {
        }

        public FileSystemState(string rootDir, FileSystemStateOptions options)
        {
            if(rootDir == null)
                throw new ArgumentException(nameof(rootDir));

            if(options == null)
                throw new ArgumentException(nameof(options));

            // This is where we hide the fact we're using DryIoc from the external caller.
            // We create a new container scope for each instance, and configure based on 
            // the options passed in.

            this.Context = s_globalContainer.OpenScope();

            // Override the Filter implementations if passed in.
            if(options.Filter != null)
                Context.Use(options.Filter);

            if(options.DirectoryFilter != null)
                Context.Use(options.DirectoryFilter);

            // Write to the EnumerationOptions; this will have been constructed for the scope but
            // the options won't have been passed in.
            var enumerationOptions = Context.Resolve<EnumerationOptions>();

            enumerationOptions.RecurseSubdirectories = options.RecurseSubdirectories;
            enumerationOptions.IgnoreInaccessible = options.IgnoreInaccessible;
            enumerationOptions.AttributesToSkip = options.AttributesToSkip;

            // The IRootDir object will already exist but not be initialised.
            // Write our chosen Root folder to it.
            this.root = Context.Resolve<IRootDir>();
            this.root.Folder = rootDir;

            // Now when we resolve we will get a scoped instance with these settings.
            this.fileSystemState = Context.Resolve<FileSystemStateInternal>();
        }

        public string RootDir => this.root.Folder;

        public void LoadState() => this.fileSystemState.LoadState();

        public IList<FileChange> GetChanges() => this.fileSystemState.GetChanges();
    }
}

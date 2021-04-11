using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DryIoc;
using YellowCounter.FileSystemState.Filter;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState
{
    /// <summary>
    /// This is the public entry point for the library.
    /// </summary>
    public class FileSystemState : IFileSystemState
    {
        // Static section, set up the container in the default configuration.
        private static IContainer s_globalContainer;

        static FileSystemState()
        {
            // Set up the DryIoc container
            s_globalContainer = new Container();
            s_globalContainer.RegisterFileSystemState();
        }

        internal IResolverContext Context { get; private set; }

        private readonly IFileSystemStateInternal fileSystemStateInternal;
        private readonly IRootDir root;

        /// <summary>
        /// Initialises the folder watcher
        /// </summary>
        /// <param name="rootDir">Root directory</param>
        /// <param name="filter">Filter pattern</param>
        /// <param name="recursive">Examine subdirectories?</param>
        public FileSystemState(string rootDir, string filter = "*", bool recursive = false)
            : this(rootDir, new FileSystemStateOptions()
                  .WithFilter(filter)
                  .WithRecurseSubdirectories(recursive))
        { }

        /// <summary>
        /// Initialises the folder watcher
        /// </summary>
        /// <param name="rootDir">Root directory</param>
        /// <param name="options">Options for filtering, recursion, file attributes</param>
        public FileSystemState(string rootDir, FileSystemStateOptions options)
        {
            if(rootDir == null)
                throw new ArgumentNullException(nameof(rootDir));

            if(options == null)
                throw new ArgumentNullException(nameof(options));

            // This is where we hide the fact we're using DryIoc from the external caller.
            // We create a new container scope for each instance, and configure based on 
            // the options passed in.

            this.Context = s_globalContainer.OpenScope();

            // Override the Filter implementations if passed in.
            if(options.Filter != null)
                this.Context.Use(options.Filter);

            if(options.DirectoryFilter != null)
                this.Context.Use(options.DirectoryFilter);

            // Write to the EnumerationOptions; this will have been constructed for the scope but
            // the options won't have been passed in.
            var enumerationOptions = this.Context.Resolve<EnumerationOptions>();

            enumerationOptions.RecurseSubdirectories = options.RecurseSubdirectories;
            enumerationOptions.IgnoreInaccessible = options.IgnoreInaccessible;
            enumerationOptions.AttributesToSkip = options.AttributesToSkip;

            // The IRootDir object will already exist but not be initialised.
            // Write our chosen Root folder to it.
            this.root = this.Context.Resolve<IRootDir>();
            this.root.Folder = rootDir;

            // Now when we resolve we will get a scoped instance with these settings.
            this.fileSystemStateInternal = this.Context.Resolve<IFileSystemStateInternal>();
        }

        /// <summary>
        /// The root directory being watched
        /// </summary>
        public string RootDir => this.root.Folder;

        /// <summary>
        /// Reads the initial state of the folder
        /// </summary>
        public void Attach() => this.fileSystemStateInternal.Attach();

        /// <summary>
        /// Returns the file changes made since the last call to Attach() or GetChanges()
        /// </summary>
        /// <returns></returns>
        public IList<FileChange> GetChanges() => this.fileSystemStateInternal.GetChanges();
    }
}

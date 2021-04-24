using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using YellowCounter.FileSystemState.Filter;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState
{
    internal class FileSystemEnumerator : FileSystemEnumerator<object>, IFileSystemEnumerator
    {
        private IAcceptFileSystemEntry acceptFileSystemEntry;

        /// <summary>
        /// Since we are inheriting from an external object, we have to play by their
        /// rules for the inner constructor signature. We convert the container-provided
        /// parameters into items suitable for the FileSystemEnumerator<>.
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="options"></param>
        /// <param name="acceptFileSystemEntry"></param>
        public FileSystemEnumerator(
            IRootDir rootDir,
            EnumerationOptions options,
            IAcceptFileSystemEntry acceptFileSystemEntry)
            : base(rootDir.Folder, options)
        {
            this.acceptFileSystemEntry = acceptFileSystemEntry;
        }

        protected override object TransformEntry(ref FileSystemEntry entry)
        {
            acceptFileSystemEntry.TransformEntry(in entry);

            return null;
        }

        protected override bool ShouldIncludeEntry(ref FileSystemEntry entry)
        {
            return acceptFileSystemEntry.ShouldIncludeEntry(in entry);
        }

        protected override bool ShouldRecurseIntoEntry(ref FileSystemEntry entry)
        {
            return acceptFileSystemEntry.ShouldRecurseIntoEntry(in entry);
        }
    }
}

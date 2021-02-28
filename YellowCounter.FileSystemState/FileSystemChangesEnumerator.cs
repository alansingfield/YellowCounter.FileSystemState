// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using YellowCounter.FileSystemState.Filter;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState
{
    internal class FileSystemChangeEnumerator : FileSystemEnumerator<object>
    {
        private readonly IFilenameFilter filter;
        private IAcceptFileSystemEntry acceptFileSystemEntry;

        public FileSystemChangeEnumerator(
            string path,
            FileSystemStateOptions options,
            IAcceptFileSystemEntry acceptFileSystemEntry)
            : base(path, options)
        {
            this.filter = options.Filter;
            this.acceptFileSystemEntry = acceptFileSystemEntry;
        }

        public void Scan()
        {
            // Enumerating causes TransformEntry() to be called repeatedly
            while(MoveNext()) { }
        }

        protected override object TransformEntry(ref FileSystemEntry entry)
        {
            acceptFileSystemEntry.Accept(in entry);

            return null;
        }

        protected override bool ShouldIncludeEntry(ref FileSystemEntry entry)
        {
            if(entry.IsDirectory)
                return false;

            return filter.ShouldInclude(entry.FileName);
        }

        protected override bool ShouldRecurseIntoEntry(ref FileSystemEntry entry) => true;
    }
}

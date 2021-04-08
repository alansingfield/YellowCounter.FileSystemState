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
    internal class FileSystemChangeEnumerator : FileSystemEnumerator<object>, IFileSystemEnumerator
    {
        private IAcceptFileSystemEntry acceptFileSystemEntry;

        public FileSystemChangeEnumerator(
            string path,
            EnumerationOptions options,
            IAcceptFileSystemEntry acceptFileSystemEntry)
            : base(path, options)
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
            return acceptFileSystemEntry.ShouldIncludeEntry(ref entry);
        }

        protected override bool ShouldRecurseIntoEntry(ref FileSystemEntry entry)
        {
            return acceptFileSystemEntry.ShouldRecurseIntoEntry(ref entry);
        }

        object IFileSystemEnumerator.TransformEntry(ref FileSystemEntry entry) => this.TransformEntry(ref entry);

        bool IFileSystemEnumerator.ShouldIncludeEntry(ref FileSystemEntry entry) => this.ShouldIncludeEntry(ref entry);

        bool IFileSystemEnumerator.ShouldRecurseIntoEntry(ref FileSystemEntry entry) => this.ShouldRecurseIntoEntry(ref entry);
    }
}

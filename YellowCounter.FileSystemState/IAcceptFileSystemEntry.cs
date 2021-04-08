﻿using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;

namespace YellowCounter.FileSystemState
{
    public interface IAcceptFileSystemEntry
    {
        void TransformEntry(in FileSystemEntry fileSystemEntry);
        bool ShouldIncludeEntry(ref FileSystemEntry entry);
        bool ShouldRecurseIntoEntry(ref FileSystemEntry entry);
    }
}

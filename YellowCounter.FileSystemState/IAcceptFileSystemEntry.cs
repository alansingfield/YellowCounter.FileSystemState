using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;

namespace YellowCounter.FileSystemState
{
    public interface IAcceptFileSystemEntry
    {
        void TransformEntry(in FileSystemEntry entry);
        bool ShouldIncludeEntry(in FileSystemEntry entry);
        bool ShouldRecurseIntoEntry(in FileSystemEntry entry);
    }
}

using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;

namespace YellowCounter.FileSystemState
{
    public interface IFileSystemEnumerator : IDisposable
    {
        //object TransformEntry(ref FileSystemEntry entry);
        //bool ShouldIncludeEntry(ref FileSystemEntry entry);
        //bool ShouldRecurseIntoEntry(ref FileSystemEntry entry);
        bool MoveNext();
    }
}

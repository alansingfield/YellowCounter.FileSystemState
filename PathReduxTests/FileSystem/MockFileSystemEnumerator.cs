using FSSUnsafe2.Mock;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;
using YellowCounter.FileSystemState;
using static FSSUnsafe2.NtDll;

namespace PathReduxTests.FileSystem
{
    /// <summary>
    /// This class is the same as FileSystemEnumerator except it does not inherit
    /// from System.IO.Enumeration.FileSystemEnumerator&lt;object%gt;
    /// </summary>
    public class MockFileSystemEnumerator: MockFileSystemEntryCall<object>, IFileSystemEnumerator
    {
        private IAcceptFileSystemEntry acceptFileSystemEntry;

        public bool IsDisposed = false;

        public MockFileSystemEnumerator(
            IAcceptFileSystemEntry acceptFileSystemEntry)
        {
            this.acceptFileSystemEntry = acceptFileSystemEntry;
        }

        public void AddFile(string filename, long length, DateTime utcModifiedDttm)
        {

        }

        public void CreateDirectory(string folder)
        {

        }

        public void Dispose()
        {
            this.IsDisposed = true;
        }

        public bool MoveNext()
        {
            CallIn(null, "hello", 12345);

            //FILE_FULL_DIR_INFORMATION info = new FILE_FULL_DIR_INFORMATION();
            //info.FileName = "hello";
            //info.LastAccessTime = DateTime.Now.Ticks;
            
            //var cheater = new Cheater();
            //cheater.Overlay.

            //acceptFileSystemEntry.ShouldIncludeEntry(in entry);
            //acceptFileSystemEntry.ShouldRecurseIntoEntry(in entry);
            //acceptFileSystemEntry.TransformEntry(in entry);

            return true;
        }

        protected override void CallOut(object context, ref FileSystemEntry fse)
        {
            acceptFileSystemEntry.ShouldIncludeEntry(in fse);
        }
    }
}

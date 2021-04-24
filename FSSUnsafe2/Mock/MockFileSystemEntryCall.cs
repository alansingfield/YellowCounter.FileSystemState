using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;
using static FSSUnsafe2.NtDll;

namespace FSSUnsafe2.Mock
{
    public unsafe abstract class MockFileSystemEntryCall<TCONTEXT>
    {
        public void CallIn(TCONTEXT context, string filename, long fileLength)
        {
            MockFileSystemEntry mockFileSystemEntry = new MockFileSystemEntry();

            var cheater = mockFileSystemEntry.Cheater;

            FILE_FULL_DIR_INFORMATION* ptr = &mockFileSystemEntry.info;
            cheater.Overlay._info = ptr;

            mockFileSystemEntry.info.FileName = filename;
            mockFileSystemEntry.info.AllocationSize = fileLength;

            CallOut(context, ref cheater.Real);
        }

        protected abstract void CallOut(TCONTEXT context, ref FileSystemEntry fse);
    }
}

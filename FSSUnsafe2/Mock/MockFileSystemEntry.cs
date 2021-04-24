using FSSUnsafe2.Interop;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;
using static FSSUnsafe2.NtDll;

namespace FSSUnsafe2.Mock
{
    public unsafe ref struct MockFileSystemEntry
    {
        public Cheater Cheater;
        public FILE_FULL_DIR_INFORMATION info;

        //public MockFileSystemEntry(int x)
        //{
        //    Cheater = new Cheater();
        //    Cheater.Overlay._info = &info;
        //}

        //public static void Init(
        //    ref MockFileSystemEntry mock,
        //    ReadOnlySpan<char> filename,
        //    long Length
        //    )
        //{
        //    mock.Cheater.Overlay._info = &mock.info;
        //}

        //public void SetFilename(ReadOnlySpan<char> value)
        //{
        //    fixed(byte* s = _buffer + sizeof(FILE_FULL_DIR_INFORMATION)) 
        //}

        

        //private unsafe FILE_FULL_DIR_INFORMATION info;

        //public MockFileSystemEntry(int x)
        //{
        //    Cheater = new Cheater();
        //    info = new FILE_FULL_DIR_INFORMATION();

        //    fixed(FILE_FULL_DIR_INFORMATION *i = &info)
        //    {
        //        Cheater.Overlay._info = i;
        //    }
        //}

        //public char* FileName
        //{
        //    get 
        //    {
        //        fixed(char* c = &info._fileName) 
        //        {
        //            return c; 
        //        }
        //    }
        //}

    }
}

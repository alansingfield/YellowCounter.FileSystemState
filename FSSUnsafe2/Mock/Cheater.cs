using FSSUnsafe2.Interop;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;

namespace FSSUnsafe2.Mock
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe ref struct Cheater
    {
        [FieldOffset(0)]
        public FileSystemEntry Real;

        [FieldOffset(0)]
        public FileSystemEntry2 Overlay;

        [FieldOffset(0)]
        public fixed byte Buffer[1000];
    }
}

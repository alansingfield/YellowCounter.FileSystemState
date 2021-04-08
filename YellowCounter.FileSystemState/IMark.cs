using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;

namespace YellowCounter.FileSystemState
{
    internal interface IMark
    {
        void TransformEntry(in FileSystemEntry input);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Filter
{
    public interface IDirectoryFilter
    {
        bool ShouldInclude(ReadOnlySpan<char> filename);
    }
}

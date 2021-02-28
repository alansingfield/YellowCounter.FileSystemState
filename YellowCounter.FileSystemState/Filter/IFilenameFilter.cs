using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Filter
{
    public interface IFilenameFilter
    {
        bool ShouldInclude(ReadOnlySpan<char> filename);
    }
}

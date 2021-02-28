using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;

namespace YellowCounter.FileSystemState.Filter
{
    public class FilenameFilter : IFilenameFilter
    {
        private static bool ignoreCase;

        static FilenameFilter()
        {
            ignoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        public string Pattern { get; set; } = "*";

        public bool ShouldInclude(ReadOnlySpan<char> filename)
        {
            return FileSystemName.MatchesSimpleExpression(this.Pattern, filename, ignoreCase);
        }
    }
}

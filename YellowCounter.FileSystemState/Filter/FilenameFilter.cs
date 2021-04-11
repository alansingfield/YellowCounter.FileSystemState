using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;

namespace YellowCounter.FileSystemState.Filter
{
    public class FilenameFilter : IFilenameFilter
    {
        protected static bool ignoreCase;

        static FilenameFilter()
        {
            ignoreCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        public virtual string Pattern { get; set; } = "*";

        public virtual bool ShouldInclude(ReadOnlySpan<char> filename)
        {
            if(this.Pattern == "*")
                return true;

            return FileSystemName.MatchesSimpleExpression(this.Pattern, filename, ignoreCase);
        }
    }
}

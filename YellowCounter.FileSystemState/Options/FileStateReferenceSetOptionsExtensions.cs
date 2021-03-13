using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Options
{
    public static class FileStateReferenceSetOptionsExtensions
    {
        public static FileStateReferenceSetOptions ApplyDefaults(this FileStateReferenceSetOptions options)
        {
            options.ReferenceSetOptions ??= new ReferenceSetOptions();
            options.ReferenceSetOptions.ApplyDefaults();

            return options;
        }
    }
}

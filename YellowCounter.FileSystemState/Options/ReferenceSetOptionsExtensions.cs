using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Options
{
    public static class ReferenceSetOptionsExtensions
    {
        public static ReferenceSetOptions ApplyDefaults(this ReferenceSetOptions options)
        {
            if(options.FillFactor <= 0)
                options.FillFactor = 70;

            ((HashBucketOptions)options).ApplyDefaults();

            return options;
        }
    }
}

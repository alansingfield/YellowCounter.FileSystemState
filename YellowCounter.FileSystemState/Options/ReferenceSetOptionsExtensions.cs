using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Sizing;

namespace YellowCounter.FileSystemState.Options
{
    public static class ReferenceSetOptionsExtensions
    {
        public static ReferenceSetOptions ApplyDefaults(this ReferenceSetOptions options)
        {
            //if(options.FillFactor <= 0)
            //    options.FillFactor = 70;

            options.HashBucketOptions ??= new HashBucketOptions();

            options.SizePolicy ??= new SizePolicy(new SizePolicyOptions());

            return options;
        }
    }
}

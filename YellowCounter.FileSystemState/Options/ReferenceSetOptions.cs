using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Sizing;

namespace YellowCounter.FileSystemState.Options
{
    public class ReferenceSetOptions
    {
        public HashBucketOptions HashBucketOptions { get; set; } = new HashBucketOptions();
        public ISizePolicy SizePolicy { get; set; } = new SizePolicy(new SizePolicyOptions());

        public ReferenceSetOptions Clone() => (ReferenceSetOptions)this.MemberwiseClone();
    }
}

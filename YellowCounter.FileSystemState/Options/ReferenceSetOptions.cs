using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Sizing;

namespace YellowCounter.FileSystemState.Options
{
    public class ReferenceSetOptions
    {
        public virtual HashBucketOptions HashBucketOptions { get; set; } = new HashBucketOptions();
        public virtual ISizePolicy SizePolicy { get; set; } = new SizePolicy();

        public virtual ReferenceSetOptions Clone() => (ReferenceSetOptions)this.MemberwiseClone();
    }
}

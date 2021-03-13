using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Options
{
    public class ReferenceSetOptions
    {
        public int? FillFactor { get; set; }
        public HashBucketOptions HashBucketOptions { get; set; }
    }
}

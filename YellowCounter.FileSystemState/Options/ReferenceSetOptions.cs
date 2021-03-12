using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Options
{
    public class ReferenceSetOptions : HashBucketOptions
    {
        public int? FillFactor { get; set; }
    }
}

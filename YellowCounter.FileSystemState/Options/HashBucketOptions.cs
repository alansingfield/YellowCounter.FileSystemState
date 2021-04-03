using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Options
{
    public class HashBucketOptions
    {
        /// <summary>
        /// Maximum number of elements which can be stored
        /// </summary>
        public virtual int Capacity { get; set; } = 256;
        
        /// <summary>
        /// Number of elements to group together which will share a common
        /// maximum known probe depth.
        /// </summary>
        public virtual int ChunkSize { get; set; } = 32;

        public virtual HashBucketOptions Clone() => (HashBucketOptions)this.MemberwiseClone();
    }
}

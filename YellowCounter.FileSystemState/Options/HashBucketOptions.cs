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
        public int Capacity { get; set; }
        
        /// <summary>
        /// Number of elements to group together which will share a common
        /// maximum known probe depth.
        /// </summary>
        public int ChunkSize { get; set; }
    }
}

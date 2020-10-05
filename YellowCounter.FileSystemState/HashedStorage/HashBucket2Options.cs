using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public class HashBucket2Options
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

        /// <summary>
        /// Permutation function to derive the second hash from the first.
        /// If null, this defaults to a non-repeating pseudo-random
        /// sequence.
        /// </summary>
        public Func<int, int> Permute { get; set; }
    }
}

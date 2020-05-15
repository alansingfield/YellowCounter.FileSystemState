using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.PathRedux
{
    public static class HashBucketExtensions
    {
        public static HashBucket Rebuild(this HashBucket source, 
            IEnumerable<(int hash, int value)> itemsEnumerator)
        {
            var factors = new List<(double capacityFactor, double linearFactor)>();

            if(source.UsageCount < source.Capacity * 0.3)
            {
                // Using less than 30% of capacity, increase linear search and reduce
                // size.
                factors.Add((0.5, 2));
            }
            else if(source.UsageCount > source.Capacity * 0.7)
            {
                // Increase size by root2 once we are using more than 70%
                // Capacity times root2, linear search factor divided by root2
                factors.Add((1.4, 0.7));
            }
            else
            {
                // Is our maximum linear search 10% of the search space or less?
                // We can retain our original capacity, just increase linear search length
                // This is often just a peak in hash collisions.
                if(source.LinearSearchLimit < source.Capacity * 0.1)
                {
                    factors.Add((1.0, 1.4));
                }
                else
                {
                    factors.Add((1.4, 1.0));
                }
            }

            // If the first try doesn't cut it, increase both the capacity and the linear
            // search limit.
            factors.Add((1.4, 1.4));

            foreach(var (capacityFactor, linearFactor) in factors)
            {
                // Adjust original size by the chosen factors
                int newCapacity = (int)(Math.Ceiling(source.Capacity * capacityFactor));
                int newLinearSearchLimit = (int)(Math.Ceiling(source.LinearSearchLimit * linearFactor));

                // Sanity limits
                if(newCapacity < source.UsageCount)
                    newCapacity = source.UsageCount + 1;

                if(newLinearSearchLimit < 1)
                    newLinearSearchLimit = 1;

                // Can't linear search for more than the overall capacity!
                if(newLinearSearchLimit > newCapacity)
                    newLinearSearchLimit = newCapacity;

                // Rebuild data in the new object
                var replacement = rebuildInternal(
                    itemsEnumerator,
                    newCapacity,
                    newLinearSearchLimit);

                if(replacement != null)
                    return replacement;
            }

            throw new Exception("Too many hash collisions.");
        }

        private static HashBucket rebuildInternal(
            IEnumerable<(int hash, int value)> itemsEnumerator,
            int capacity, int chain)
        {
            var newLookup = new HashBucket(capacity, chain);

            // Populate a new lookup from our existing data.
            foreach(var (hash, value) in itemsEnumerator)
            {
                // Too many hash collisions? Need to try new linear search limit?
                if(!newLookup.Store(hash, value))
                    return null;
            }

            return newLookup;
        }
    }
}

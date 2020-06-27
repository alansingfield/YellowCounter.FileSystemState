using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.PathRedux
{
    public static class HashBucketExtensions
    {
        public static IEnumerable<HashBucketOptions> SizeOptions<T>(this HashBucket<T> source, int headroom = 0)
        {
            int requiredSize = source.Occupancy + headroom;

            if(requiredSize < 0)
                throw new ArgumentException($"Headroom change must not be less than current occupancy", nameof(headroom));

            var factors = new List<(double capacityFactor, double linearFactor)>();

            if(requiredSize < source.Capacity * 0.3)
            {
                // Using less than 30% of capacity, increase linear search and reduce
                // size.
                factors.Add((0.5, 2));
            }
            else if(requiredSize > source.Capacity * 0.7)
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

                // Must have at least enough space for the current usage count and extra
                // headroom requested.
                if(newCapacity < requiredSize)
                    newCapacity = requiredSize;

                // Must linear search at least one item
                if(newLinearSearchLimit < 1)
                    newLinearSearchLimit = 1;

                // Can't linear search for more than the overall capacity!
                if(newLinearSearchLimit > newCapacity)
                    newLinearSearchLimit = newCapacity;

                yield return new HashBucketOptions()
                {
                    Capacity = newCapacity,
                    LinearSearchLimit = newLinearSearchLimit
                };
            }

        }

        //public static double AvgLinearSearch(this HashBucket<T> source) => (double)source.LinearSearchCount / source.Occupancy;

    }
}

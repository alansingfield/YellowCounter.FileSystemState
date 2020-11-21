using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public static class HashBucketExtensions
    {
        public static bool TryStore<T>(this HashBucket<T> source, int hash, T value)
        {
            return source.TryStore(hash, value, out int _);
        }

        public static List<T> ToList<T>(this HashBucket<T> source)
        {
            var result = new List<T>(source.Usage);

            foreach(var itm in source)
            {
                result.Add(itm);
            }

            return result;
        }

        public static T[] ToArray<T>(this HashBucket<T> source)
        {
            var result = new T[source.Usage];

            int idx = 0;
            foreach(var itm in source)
            {
                result[idx] = itm;
                idx++;
            }

            return result;
        }

        public static IEnumerable<HashBucketOptions> SizeOptions<T>(this HashBucket<T> source, int headroom = 0)
        {
            int requiredSize = source.Occupancy + headroom;

            if(requiredSize < 0)
                throw new ArgumentException($"Headroom change must not be less than current occupancy", nameof(headroom));

            var factors = new List<double>();

            if(requiredSize < source.Capacity * 0.3)
            {
                // Using less than 30% of capacity, reduce size.
                factors.Add(0.5);
            }
            else if(requiredSize > source.Capacity * 0.7)
            {
                // Increase size by root2 once we are using more than 70%
                // Capacity times root2
                factors.Add(1.4);
            }

            foreach(var capacityFactor in factors)
            {
                // Adjust original size by the chosen factors
                int newCapacity = (int)(Math.Ceiling(source.Capacity * capacityFactor));

                // Sanity limits

                // Must have at least enough space for the current usage count and extra
                // headroom requested.
                if(newCapacity < requiredSize)
                    newCapacity = requiredSize;

                yield return new HashBucketOptions()
                {
                    Capacity = newCapacity,
                    ChunkSize = source.ChunkSize,
                };
            }

        }

    }
}

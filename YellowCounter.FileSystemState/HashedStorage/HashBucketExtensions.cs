using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Options;

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
    }
}

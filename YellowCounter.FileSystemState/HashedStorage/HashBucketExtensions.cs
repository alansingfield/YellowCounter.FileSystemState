using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public static class HashBucketExtensions
    {

        /// <summary>
        /// Stores value against the specified hash. Multiple values can be stored
        /// against the same hash (it does not overwrite).
        /// 
        /// DOES NOT RAISE AN EXCEPTION if it can't store the value.
        /// You must check the return value.
        /// </summary>
        /// <param name="hash">Hashcode to store</param>
        /// <param name="value">Value to store</param>
        /// <returns>True if storing worked.</returns>
        public static bool TryStore<T>(this HashBucket<T> source, int hash, T value)
        {
            return source.TryStore(hash, value, out int _);
        }

        /// <summary>
        /// Copies the contents of the HashBucket to a list. Deleted items
        /// are excluded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this HashBucket<T> source)
        {
            var result = new List<T>(source.Usage);

            foreach(var itm in source)
            {
                result.Add(itm);
            }

            return result;
        }

        /// <summary>
        /// Copies the contents of the HashBucket to an array. Deleted items
        /// are excluded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
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

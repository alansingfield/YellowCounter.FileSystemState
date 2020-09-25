using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public static class ReferenceSetExtensions
    {
        /// <summary>
        /// Take a copy of all the values in the set
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<TValue> ToList<TKey, TValue>(this ReferenceSet<TKey, TValue> source)
            where TValue : struct
            where TKey : struct
        {
            var result = new List<TValue>(source.Usage);

            foreach(var itm in source)
            {
                result.Add(itm);
            }

            return result;
        }

        /// <summary>
        /// Take a copy of all the values in the set
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TValue[] ToArray<TKey, TValue>(this ReferenceSet<TKey, TValue> source)
            where TValue : struct
            where TKey : struct
        {
            var result = new TValue[source.Usage];

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

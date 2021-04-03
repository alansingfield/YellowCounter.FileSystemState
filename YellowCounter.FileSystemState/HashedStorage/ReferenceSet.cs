using System;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState.HashedStorage
{


    public abstract class ReferenceSet<TKey, TValue> : IDisposable
        where TValue: struct 
        where TKey: struct
    {
        private ReferenceSetOptions options;
        private HashBucket<TValue> hashBucket;

        public ReferenceSet(ReferenceSetOptions options = null)
        {
            this.options = options == null ? new ReferenceSetOptions()
                : options.Clone();

            hashBucket = new HashBucket<TValue>(this.options.HashBucketOptions);
        }

        /// <summary>
        /// Calculates the key of a given item. Must be deterministic.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract TKey GetKey(TValue item);

        /// <summary>
        /// Gets the hash for a given key. Note that the default implementation
        /// of GetHashCode() gives a poor quality result for structs, it is
        /// recommended to write a custom function using HashCode.Combine().
        /// </summary>
        /// <param name="key"></param>
        /// <returns>integer hashcode</returns>
        protected abstract int GetHashOfKey(TKey key);

        /// <summary>
        /// Comparison function, should return true if <paramref name="item"/>
        /// matches <paramref name="key"/>. This will be called during most
        /// retrieval operations (including indexing with []) to discard the
        /// hash collisions.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected abstract bool Match(TValue item, TKey key);

        public int GetHashOfValue(TValue item) => GetHashOfKey(GetKey(item));

        /// <summary>
        /// Adds a new item to the set. If the key is already in the set,
        /// an ArgumentException will be raised.
        /// </summary>
        /// <param name="key">Unique key to store against</param>
        /// <param name="value">Value to store</param>
        /// <param name="index">Index where item was stored</param>
        /// <returns>Reference to the added item within this set</returns>
        public ref TValue Add(TKey key, TValue value, out int index)
        {
            int hash = GetHashOfKey(key);

            if(containsKeyInternal(key, hash))
                throw new ArgumentException("The key is already in the set", nameof(key));

            return ref tryStoreInternal(value, hash, out index);
        }

        private static int dummy;
        public ref TValue Add(TKey key, TValue value)
        {
            // The dummy variable will get overwritten by each caller, but as it is
            // not returned, I can't see this being a problem.
            return ref Add(key, value, out dummy);
        }

        private ref TValue tryStoreInternal(TValue value, int hash, out int index)
        {
            resizeIfNeeded(1);

            if(!hashBucket.TryStore(hash, value, out index))
                throw new Exception("HashBucket was resized but there was still not enough room");

            return ref hashBucket[index];
        }

        private void resizeIfNeeded(int headroom)
        {
            int? newCapacity = options.SizePolicy.MustResize(
                this.hashBucket.Usage + headroom,
                this.hashBucket.Capacity);

            if(newCapacity != null)
            {
                Resize(newCapacity.Value);
            }
        }

        private bool hashAndMatch(TKey key, int hash, TValue item)
        {
            // First compare by hash, as the HashBucket can give us items
            // which do not match the hash. Then run the Match() function
            // to avoid hash collisions and identify the correct item.

            return GetHashOfValue(item) == hash && Match(item, key);
        }

        public ref TValue this[TKey key]
        {
            get
            {
                int hash = GetHashOfKey(key);

                foreach(ref TValue item in hashBucket.Retrieve(hash))
                {
                    if(hashAndMatch(key, hash, item))
                        return ref item;
                }

                throw new ArgumentException("Key was not found in the set", nameof(key));
            }
        }

        public int IndexOf(TKey key)
        {
            int hash = GetHashOfKey(key);

            foreach(var idx in hashBucket.RetrieveIndices(hash))
            {
                TValue item = hashBucket[idx];

                if(hashAndMatch(key, hash, item))
                {
                    return idx;
                }
            }
            return -1;
        }

        public ref TValue ElementAt(int index) => ref hashBucket[index];


        public void DeleteAt(int idx)
        {
            hashBucket.DeleteAt(idx);
        }

        public bool ContainsKey(TKey key)
        {
            int hash = GetHashOfKey(key);

            return containsKeyInternal(key, hash);
        }

        private bool containsKeyInternal(TKey key, int hash)
        {
            foreach(ref TValue item in hashBucket.Retrieve(hash))
            {
                if(hashAndMatch(key, hash, item))
                {
                    return true;
                }
            }
            return false;
        }

        public HashBucket<TValue>.Enumerator GetEnumerator()
        {
            return hashBucket.GetEnumerator();
        }


        public void Resize(int newCapacity)
        {
            var replacement = new HashBucket<TValue>(new HashBucketOptions()
            {
                Capacity = newCapacity,
                ChunkSize = hashBucket.ChunkSize,
            });

            // Populate the replacement with our existing data
            foreach(ref var itm in hashBucket)
            {
                if(!replacement.TryStore(GetHashOfKey(GetKey(itm)), itm))
                    throw new Exception("Unable to resize");
            }

            this.hashBucket.Dispose();
            this.hashBucket = replacement;
        }

        public void Dispose()
        {
            ((IDisposable)hashBucket)?.Dispose();
        }

        public HashBucket<TValue>.IndexSegment AllIndices()
        {
            return hashBucket.AllIndices();
        }

        /// <summary>
        /// Overall number of slots
        /// </summary>
        public int Capacity => hashBucket.Capacity;
        /// <summary>
        /// Number of slots we are using at the moment (including soft deleted)
        /// </summary>
        public int Occupancy => hashBucket.Occupancy;
        /// <summary>
        /// Number of used slots excluding soft-deleted
        /// </summary>
        public int Usage => hashBucket.Usage;
    }
}



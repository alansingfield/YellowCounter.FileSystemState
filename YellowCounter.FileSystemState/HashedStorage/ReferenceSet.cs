using System;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public class SetByRefOptions : HashBucketOptions
    {
        public int? FillFactor { get; set; }
    }

    public abstract class ReferenceSet<TKey, TValue> : IDisposable
        where TValue: struct 
        where TKey: struct
    {
        private HashBucket<TValue> hashBucket;
        private float fillFactor;
        private int usageLimit;

        public ReferenceSet(SetByRefOptions options = null)
        {
            options ??= new SetByRefOptions()
            {
                Capacity = 256,
            };

            hashBucket = new HashBucket<TValue>(options);

            this.fillFactor = (options.FillFactor ?? 80) / 100.0f;

            refreshUsageLimit();
        }

        private void refreshUsageLimit()
        {
            this.usageLimit = (int)(hashBucket.Capacity * this.fillFactor);
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
        /// <returns>Reference to the added item within this set</returns>
        public ref TValue Add(TKey key, TValue value)
        {
            int hash = GetHashOfKey(key);

            if(containsKeyInternal(key, hash))
                throw new ArgumentException("The key is already in the set", nameof(key));

            return ref tryStoreInternal(value, hash);
        }


        private ref TValue tryStoreInternal(TValue value, int hash)
        {
            // make sure we have not exceeded the fillfactor limit.
            if(hashBucket.Usage < usageLimit)
            {
                if(hashBucket.TryStore(hash, value, out int position))
                {
                    return ref hashBucket[position];
                }
            }

            // Rebuild the HashBucket and store the new item in there. This will
            // throw an exception if after resizing it still can't fit in - not
            // sure how that could happen...
            rebuildLookup(headroom: 1, value, out int newPosition);

            return ref hashBucket[newPosition];
        }


        ///// <summary>
        ///// Finds the item with the given key. If the item can't be found, calls
        ///// valueFactory() to create a new item, and stores this under the key
        ///// provided.
        ///// </summary>
        ///// <param name="key">Key to search for.</param>
        ///// <param name="valueFactory">Creates the <typeparamref name="TValue"/>
        ///// if not found in existing set</param>
        ///// <returns>Reference to the found or newly created item.</returns>
        //public ref TValue GetOrAdd<TContext>(TKey key, Func<TContext, TValue> valueFactory, TContext context)
        //{
        //    int hash = GetHashOfKey(key);

        //    foreach(ref TValue item in hashBucket.Retrieve(hash))
        //    {
        //        if(hashAndMatch(key, hash, item))
        //        {
        //            return ref item;
        //        }
        //    }

        //    // Not found, call the factory method to create the item.
        //    TValue newItem = valueFactory(context);

        //    return ref tryStoreInternal(newItem, hash);
        //}

        private bool hashAndMatch(TKey key, int hash, TValue item)
        {
            // First compare by hash, as the HashBucket can give us items
            // which do not match the hash. Then run the Match() function
            // to avoid hash collisions and identify the correct item.

            return GetHashOfValue(item) == hash && Match(item, key);
        }


        ///// <summary>
        ///// Calculates the Key of the argument, and then looks for an existing
        ///// item which matches that key. If it finds one, the old item is
        ///// overwritten in-place with the new one. If it does not find the
        ///// item the supplied value is added to the Set.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns>Reference to the item within this ReferenceSet</returns>
        //public ref TValue AddOrReplace(TValue value)
        //{
        //    TKey key = GetKey(value);
        //    int hash = GetHashOfKey(key);

        //    // If the key is already in our HashBucket, overwrite the slot
        //    // with the supplied value.
        //    foreach(ref TValue item in hashBucket.Retrieve(hash))
        //    {
        //        if(hashAndMatch(key, hash, item))
        //        {
        //            // Copy-write the supplied value into our array
        //            item = value;

        //            // Return reference to the item in the array
        //            return ref item;
        //        }
        //    }

        //    // Item not yet in the HashBucket, so add it in and return
        //    // reference to item created.
        //    return ref tryStoreInternal(value, hash);
        //}

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

            foreach(ref TValue item in hashBucket.Retrieve(hash))
            {
                if(hashAndMatch(key, hash, item))
                {
                    return hashBucket.IndexOf(ref item);
                }
            }
            return -1;
        }

        public ref TValue ElementAt(int index) => ref hashBucket[index];

        public bool Delete(TKey key)
        {
            int hash = GetHashOfKey(key);

            foreach(ref TValue item in hashBucket.Retrieve(hash))
            {
                if(hashAndMatch(key, hash, item))
                {
                    hashBucket.Delete(ref item);
                    return true;
                }
            }
            return false;
        }

        public void Delete(ref TValue value)
        {
            hashBucket.Delete(ref value);
        }

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
            
            refreshUsageLimit();
        }

        private void rebuildLookup(int headroom, TValue newItem, out int position)
        {
            // Given the headroom we need, get a list of possible sizes to try for the lookup.
            foreach(var opts in hashBucket.SizeOptions(headroom))
            {
                // Create a replacement hash bucket of the new size
                var replacement = new HashBucket<TValue>(opts);

                // Populate the replacement with our existing data
                foreach(ref var itm in hashBucket)
                {
                    if(!replacement.TryStore(GetHashOfKey(GetKey(itm)), itm))
                        continue;   // Can't store in the replacement, try a different size
                }

                // Store the new value which made us exceed the size threshold.
                if(!replacement.TryStore(GetHashOfKey(GetKey(newItem)), newItem, out position))
                    continue;

                // We've managed to store everything, REPLACE the old lookup with a new one.
                this.hashBucket.Dispose();
                this.hashBucket = replacement;

                refreshUsageLimit();
                return;
            }

            throw new Exception("Unable to rebuild HashBucket");
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



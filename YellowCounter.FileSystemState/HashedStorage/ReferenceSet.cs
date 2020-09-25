using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public class SetByRefOptions : HashBucket2Options
    {
    }

    public abstract class ReferenceSet<TKey, TValue> 
        where TValue: struct 
        where TKey: struct
    {
        private HashBucket2<TValue> hashBucket;

        public ReferenceSet(SetByRefOptions options = null)
        {
            hashBucket = new HashBucket2<TValue>(options ?? new SetByRefOptions()
            {
                Capacity = 256,
                LinearSearchLimit = 16
            });
        }

        protected abstract TKey GetKey(in TValue item);
        protected abstract int GetHashOfKey(in TKey key);
        protected abstract bool Match(in TValue item, in TKey key);

        public ref TValue Add(TKey key, TValue value)
        {
            int hash = GetHashOfKey(key);

            if(containsKeyInternal(key, hash))
                throw new ArgumentException("The key is already in the set, use GetOrAdd/AddOrReplace instead", nameof(key));

            return ref tryStoreInternal(value, hash);
        }

        private ref TValue tryStoreInternal(TValue value, int hash)
        {
            if(hashBucket.TryStore(hash, value, out int position))
            {
                return ref hashBucket[position];
            }

            // Rebuild the HashBucket and store the new item in there. This will
            // throw an exception if after resizing it still can't fit in - not
            // sure how that could happen...
            rebuildLookup(headroom: 1, value, out int newPosition);

            return ref hashBucket[newPosition];
        }



        //public abstract void OnUpdating(TKey key, context, ref TValue value);
        //public abstract TValue OnCreate(TKey key, context);

        /// <summary>
        /// Finds the item with the given key. If the item can't be found, calls
        /// valueFactory() to create a new item, and stores this under the key
        /// provided.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns>Reference to the found or newly created item.</returns>
        public ref TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
        {
            int hash = GetHashOfKey(key);

            foreach(ref TValue item in hashBucket.Retrieve(hash))
            {
                if(Match(item, key))
                    return ref item;
            }

            TValue newItem = valueFactory();

            return ref tryStoreInternal(newItem, hash);
        }

        public ref TValue AddOrReplace(TValue value)
        {
            TKey key = GetKey(value);
            int hash = GetHashOfKey(key);

            // If the key is already in our HashBucket, overwrite the slot
            // with the supplied value
            foreach(ref TValue item in hashBucket.Retrieve(hash))
            {
                if(Match(item, key))
                {
                    // Copy-write the supplied value into our array
                    item = value;

                    // Return reference to the item in the array
                    return ref item;
                }
            }

            // Item not yet in the HashBucket, so add it in and return
            // reference to item created.
            return ref tryStoreInternal(value, hash);
        }

        public ref TValue this[TKey key]
        {
            get
            {
                int hash = GetHashOfKey(key);

                foreach(ref TValue item in hashBucket.Retrieve(hash))
                {
                    if(Match(item, key))
                        return ref item;
                }

                throw new ArgumentException("Key was not found in the set", nameof(key));
            }
        }

        public bool TryGet(in TKey key, ref TValue result)
        {
            int hash = GetHashOfKey(key);

            foreach(ref TValue item in hashBucket.Retrieve(hash))
            {
                if(Match(item, key))
                {
                    result = ref item;
                    return true;
                }
            }
            return false;
        }

        public bool Delete(in TKey key)
        {
            int hash = GetHashOfKey(key);

            foreach(ref TValue item in hashBucket.Retrieve(hash))
            {
                if(Match(item, key))
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

        public bool ContainsKey(in TKey key)
        {
            int hash = GetHashOfKey(key);

            return containsKeyInternal(key, hash);
        }

        private bool containsKeyInternal(TKey key, int hash)
        {
            foreach(ref TValue item in hashBucket.Retrieve(hash))
            {
                if(Match(item, key))
                {
                    return true;
                }
            }
            return false;
        }

        public HashBucket2<TValue>.Enumerator GetEnumerator()
        {
            return hashBucket.GetEnumerator();
        }


        private void rebuildLookup(int headroom, in TValue newItem, out int position)
        {
            // Given the headroom we need, get a list of possible sizes to try for the lookup.
            foreach(var opts in hashBucket.SizeOptions(headroom))
            {
                // Create a replacement hash bucket of the new size
                var replacement = new HashBucket2<TValue>(opts);

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
                this.hashBucket = replacement;
                return;
            }

            throw new Exception("Unable to rebuild HashBucket");
        }

        /// <summary>
        /// Overall number of slots
        /// </summary>
        public int Capacity => hashBucket.Capacity;
        /// <summary>
        /// Maximum possible linear search we will undertake
        /// </summary>
        public int LinearSearchLimit => hashBucket.LinearSearchLimit;
        /// <summary>
        /// Number of slots we are using at the moment (including soft deleted)
        /// </summary>
        public int Occupancy => hashBucket.Occupancy;
        /// <summary>
        /// Number of used slots excluding soft-deleted
        /// </summary>
        public int Usage => hashBucket.Usage;
        /// <summary>
        /// Longest linear search we've had to do. Starts at zero with nothing
        /// stored. Maximum possible value will be same as LinearSearchLimit.
        /// </summary>
        public int MaxLinearSearch => hashBucket.MaxLinearSearch;
    }
}





//public void CreateOrUpdate(TKey key, TContext context)
//{
//    int hash = GetHashOfKey(key);

//    foreach(ref TValue item in hashBucket.Retrieve(hash))
//    {
//        if(Match(item, key))
//        {
//            OnUpdating(key, context, ref item);
//            return;
//        }
//    }

//    // Item not found, so call
//    TValue newItem = OnCreate(key, context);

//    if(hashBucket.TryStore(hash, newItem))
//        return;

//    if(rebuildLookup(headroom: 1, in newItem, out int position))
//        return;

//    // Theoretically this shouldn't happen, but...
//    // We've got a backstop which increases both the capacity and the linear search
//    // limit - what more could we do? Let's find out...
//    throw new Exception("Too many hash collisions.");
//}
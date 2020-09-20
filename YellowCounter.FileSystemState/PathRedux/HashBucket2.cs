using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class HashBucket2Options
    {
        public int Capacity { get; set; }
        public int LinearSearchLimit { get; set; }
    }

    /// <summary>
    /// A hash bucket using the Open Addressing scheme. This is a fixed capacity
    /// implementation.
    /// </summary>
    public class HashBucket2<T>
    {
        private T[] mem;
        private readonly int capacity;
        private readonly int linearSearchLimit;
        private readonly BitArray elementsInUse;
        private readonly BitArray softDeleted;
        private readonly int sizeofT;
        private int occupancy;
        private int usage;
        private int maxLinearSearch;

        public HashBucket2(HashBucket2Options options) : this(options.Capacity, options.LinearSearchLimit) { }

        public HashBucket2(int capacity, int linearSearchLimit)
        {
            mem = new T[capacity];
            elementsInUse = new BitArray(capacity);
            softDeleted = new BitArray(capacity);
            occupancy = 0;
            usage = 0;

            this.capacity = capacity;
            this.linearSearchLimit = linearSearchLimit;

            this.sizeofT = Unsafe.SizeOf<T>();
        }

        /// <summary>
        /// Overall number of slots
        /// </summary>
        public int Capacity => this.capacity;
        /// <summary>
        /// Maximum possible linear search we will undertake
        /// </summary>
        public int LinearSearchLimit => this.linearSearchLimit;
        /// <summary>
        /// Number of slots we are using at the moment (including soft deleted)
        /// </summary>
        public int Occupancy => this.occupancy;
        /// <summary>
        /// Number of used slots excluding soft-deleted
        /// </summary>
        public int Usage => this.usage;
        /// <summary>
        /// Longest linear search we've had to do. Starts at zero with nothing
        /// stored. Maximum possible value will be same as LinearSearchLimit.
        /// </summary>
        public int MaxLinearSearch => this.maxLinearSearch;

        /// <summary>
        /// Stores value against the specified hash. Multiple values can be stored
        /// against the same hash (it does not overwrite).
        /// 
        /// DOES NOT RAISE AN EXCEPTION if it can't store the value.
        /// You must check the return value.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="value"></param>
        /// <param name="index">Outputs position of where value was stored. Will be set to -1 if store fails.</param>
        /// <returns>True if storing worked.</returns>
        public virtual bool TryStore(int hash, T value, out int index)
        {
            // Calculate which is the first slot we should try
            var cursor = new Cursor(
                slotFromHash(hash),
                this.capacity,
                this.linearSearchLimit);

            bool foundSlot = false;

            // Starting at the first slot, search for a free slot to put our
            // data into. We might shoot past the end of capacity so
            // using the cursor loops around back to the start.
            while(cursor.MoveNext())
            {
                if(!elementsInUse[cursor.Index] || softDeleted[cursor.Index])
                {
                    foundSlot = true;
                    break;
                }
            }

            if(!foundSlot)
            {
                index = -1;
                return false;
            }

            // Write to the memory and our "in use" bit array.
            mem[cursor.Index] = value;
            elementsInUse[cursor.Index] = true;

            this.usage++;

            // Are we re-using an existing slot that was soft deleted?
            if(softDeleted[cursor.Index])
            {
                softDeleted[cursor.Index] = false;
            }
            else
            {
                // Using a new slot
                this.occupancy++;
            }

            // Keep track of the longest linear search we've had to do
            // so far.
            if(maxLinearSearch <= cursor.MoveCount)
                maxLinearSearch = cursor.MoveCount + 1;

            // Return the position we stored the item at.
            index = cursor.Index;
            return true;
        }

        public void DeleteAt(int position)
        {
            // We will be in range if item is a member of the mem array.
            if(position < 0 || position >= softDeleted.Length)
                throw new IndexOutOfRangeException();

            deleteAtInternal(position);
        }

        private void deleteAtInternal(int position)
        {
            if(!softDeleted[position])
            {
                softDeleted[position] = true;
                this.usage--;
            }
        }

        public void Delete(ref T item)
        {
            DeleteAt(PositionOf(ref item));
        }

        public int PositionOf(ref T item)
        {
            // Calculate the array index of item within mem
            return (int)Unsafe.ByteOffset(
                ref this.mem[0], ref item)
                / this.sizeofT;
        }

        /// <summary>
        /// This logic controls the cursor position for scanning through a wrap-around
        /// array.
        /// </summary>
        private struct Cursor
        {
            /// <summary>
            /// Calculates a moduloed index within a wrap-around array
            /// </summary>
            /// <param name="startIndex">Start index in array, from 0..capacity-1</param>
            /// <param name="capacity">Length of array</param>
            /// <param name="moveLimit">Limit the number of iterations to avoid infinite loop</param>
            public Cursor(int startIndex, int capacity, int moveLimit)
            {
                if(startIndex < 0 || startIndex >= capacity)
                    throw new ArgumentException(
                        $"{nameof(startIndex)} must be between 0 and ${nameof(capacity)}-1", 
                        nameof(startIndex));

                if(moveLimit < 0)
                    throw new ArgumentException(
                        $"{nameof(moveLimit)} must be >= 0", nameof(moveLimit));

                if(capacity < 0)
                    throw new ArgumentException(
                        $"{nameof(capacity)} must be >= 0", nameof(capacity));

                this.Capacity = capacity;
                this.MoveLimit = moveLimit;

                this.Index = startIndex;
                this.MoveCount = 0;
                this.Started = false;
                this.Ended = false;
            }

            /// <summary>
            /// Advance the cursor. The first call sets Started to true. Subsequent calls increase
            /// the Index and MoveCount fields. The Index field wraps around, the MoveCount does not.
            /// When we make the (MoveLimit+1)st move, it will return False and the Ended flag is
            /// set to true. Any further calls will always return false and leave the index where it is.
            /// </summary>
            /// <returns>True if we have not exceeded maximum number of iterations (MoveLimit)</returns>
            public bool MoveNext()
            {
                if(Ended)
                    return false;

                if(!Started)
                {
                    Started = true;
                }
                else
                {
                    MoveCount++;
                    Index++;
                }

                if(Index >= Capacity)
                    Index %= Capacity;

                var result = MoveCount < MoveLimit;

                if(!result)
                    Ended = true;

                return result;
            }

            /// <summary>
            /// Have we moved the cursor yet? True after first call to MoveNext()
            /// </summary>
            public bool Started { get; private set; }
            /// <summary>
            /// Have we reached the limit of the number of moves?
            /// </summary>
            public bool Ended { get; private set; }

            /// <summary>
            /// Array index, will be in range 0..Capacity-1
            /// </summary>
            public int Index { get; private set; }

            public int MoveCount { get; private set; }

            public int Capacity { get; }

            public int MoveLimit { get; }
        }

        /// <summary>
        /// Modulo divide the hash by our capacity
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private int slotFromHash(int hash) => (int)unchecked((uint)hash % (uint)Capacity);

        public Segment Retrieve(int hash)
        {
            // Calculate the first spot our result could be in
            int slot = slotFromHash(hash);

            // Enumerate from this slot onwards.
            return new Segment(new Enumerator(
                mem,
                elementsInUse,
                softDeleted,
                slot,
                maxLinearSearch,
                capacity,
                contiguous: true));
        }

        public ref T this[int index]
        {
            get
            {
                // Verify that the element is available and not soft deleted
                if(!elementsInUse[index])
                    throw new ArgumentOutOfRangeException();

                if(softDeleted[index])
                    throw new ArgumentOutOfRangeException();

                return ref mem[index];
            }
        }

        /// <summary>
        /// Returns true if an element exists at the given index position.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool ExistsAt(int index)
        {
            if(index < 0)
                return false;

            if(index >= this.Capacity)
                return false;

            if(!elementsInUse[index])
                return false;

            if(softDeleted[index])
                return false;

            return true;
        }

        public ref struct Segment
        {
            Enumerator enumerator;

            public Segment(Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public Enumerator GetEnumerator() => this.enumerator;
        }

        public Enumerator GetEnumerator()
        {
            // This enumerates through every available, non-soft-deleted item
            // in the set.

            // If there are zero elements, don't make the enumerator look through the
            // entire array to confirm this. If one or more elements we must enumerate
            // every element.
            int scanLimit = this.occupancy == 0 ? 0 : this.capacity;

            return new Enumerator(
                mem, 
                this.elementsInUse, 
                this.softDeleted, 
                0,                  // Start at the beginning
                scanLimit,          // Either 0 or all the elements
                capacity,           // End at the end
                contiguous: false); 
        }

        public ref struct Enumerator
        {
            private Cursor cursor;

            private readonly T[] mem;
            private readonly BitArray elementsInUse;
            private readonly BitArray softDeleted;
            private bool contiguous;

            public Enumerator(
                T[] mem, 
                BitArray elementsInUse, 
                BitArray softDeleted, 
                int start, 
                int scanLimit, 
                int capacity,
                bool contiguous)
            {
                this.mem = mem;
                this.elementsInUse = elementsInUse;
                this.softDeleted = softDeleted;
                this.contiguous = contiguous;

                this.cursor = new Cursor(
                    start,
                    capacity,
                    scanLimit);
            }

            public ref T Current
            {
                get
                {
                    if(!cursor.Started)
                        throw new InvalidOperationException();

                    return ref mem[cursor.Index];
                }
            }

            public bool MoveNext()
            {
                while(cursor.MoveNext())
                {
                    // The cursor can be used in two ways. If contiguous, we must have
                    // a continuous sequence of elements all of which are "in use".
                    // The enumeration will stop as soon as we hit an element which has
                    // never been used.
                    // If non-contiguous, we skip over elements which are not "in use"
                    // and carry on until we find an "in use" element.
                    if(!elementsInUse[cursor.Index])
                    {
                        if(contiguous)
                            return false;
                        else
                            continue;
                    }

                    // Skip over soft deleted items.
                    if(softDeleted[cursor.Index])
                        continue;

                    // Element is "in use" and not "soft deleted" so return it.
                    return true;
                }

                // Exhausted the maximum search length of the cursor, stop enumerating.
                return false;
            }
        }
    }
    public static class HashBucket2Extensions
    {
        public static bool TryStore<T>(this HashBucket2<T> source, int hash, T value)
        {
            return source.TryStore(hash, value, out int _);
        }

        public static List<T> ToList<T>(this HashBucket2<T> source)
        {
            var result = new List<T>(source.Usage);

            foreach(var itm in source)
            {
                result.Add(itm);
            }

            return result;
        }

        public static T[] ToArray<T>(this HashBucket2<T> source)
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

        public static IEnumerable<HashBucket2Options> SizeOptions<T>(this HashBucket2<T> source, int headroom = 0)
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

                yield return new HashBucket2Options()
                {
                    Capacity = newCapacity,
                    LinearSearchLimit = newLinearSearchLimit
                };
            }

        }

        //public static double AvgLinearSearch(this HashBucket<T> source) => (double)source.LinearSearchCount / source.Occupancy;

    }

}

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
        private int occupancy;
        private int usage;
        private int maxLinearSearch;
        private int sizeofT;

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
        /// Stores value against the specified hash. DOES NOT RAISE AN EXCEPTION
        /// if it can't store the value. You must check the return value.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="value"></param>
        /// <returns>True if storing worked.</returns>
        public virtual bool TryStore(int hash, T value, out int position)
        {
            // Calculate which is the first slot we should try
            RSM rsm = new RSM(
                slotFromHash(hash),
                this.capacity,
                this.linearSearchLimit);

            // Starting at the first slot, search for a free slot to put our
            // data into. We might shoot past the end of capacity so
            // using ModuloSlot loops around back to the start.
            while(elementsInUse[rsm.Position]
                && !softDeleted[rsm.Position])
            {
                // Inc() will return false if we have exceeded the search limit.
                if(!rsm.Inc())
                {
                    position = -1;
                    return false;
                }
            }

            // Write to the memory and our "in use" bit array.
            mem[rsm.Position] = value;
            elementsInUse[rsm.Position] = true;

            this.usage++;

            // Are we re-using an existing slot that was soft deleted?
            if(softDeleted[rsm.Position])
            {
                softDeleted[rsm.Position] = false;
            }
            else
            {
                // Using a new slot
                this.occupancy++;
            }

            // Keep track of the longest linear search we've had to do
            // so far.
            if(maxLinearSearch <= rsm.Offset)
                maxLinearSearch = rsm.Offset + 1;

            // Return the position we stored the item at.
            position = rsm.Position;
            return true;
        }

        public void DeleteAt(int position)
        {
            // We will be in range if item is a member of the mem array.
            if(position < 0 || position >= softDeleted.Length)
                throw new IndexOutOfRangeException();

            if(!softDeleted[position])
            {
                softDeleted[position] = true;
                this.usage--;
            }
        }

        public void Delete(ref T item)
        {
            // Calculate the array index of item within mem
            int position = (int)Unsafe.ByteOffset(
                ref this.mem[0], ref item) 
                / this.sizeofT;

            DeleteAt(position);
        }

        private struct RSM
        {
            private readonly int capacity;
            private readonly int scanLimit;

            public RSM(int startPosition, int capacity, int scanLimit)
            {
                this.capacity = capacity;
                this.scanLimit = scanLimit;
                this.Position = startPosition;

                this.Offset = 0;
            }

            public bool Inc()
            {
                Offset++;
                Position++;

                if(Position >= capacity)
                    Position %= capacity;

                return Offset <= scanLimit;
            }

            public int Position { get; private set; }
            public int Offset { get; private set; }
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
                capacity));
        }

        public ref T this[int index]
        {
            get
            {
                return ref mem[index];
            }
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
                capacity);          // End at the end
        }

        public ref struct Enumerator
        {
            private bool started;
            private RSM rsm;

            private readonly T[] mem;
            private readonly BitArray elementsInUse;
            private readonly BitArray softDeleted;

            public Enumerator(
                T[] mem, 
                BitArray elementsInUse, 
                BitArray softDeleted, 
                int start, 
                int scanLimit, 
                int capacity)
            {
                this.mem = mem;
                this.elementsInUse = elementsInUse;
                this.softDeleted = softDeleted;

                this.started = false;

                this.rsm = new RSM(
                    start - 1,  // Take account of first call to Inc() in MoveNext()
                    capacity,
                    scanLimit);
            }

            public ref T Current
            {
                get
                {
                    if(!started)
                        throw new InvalidOperationException();

                    return ref mem[rsm.Position];
                }
            }

            public bool MoveNext()
            {
                if(!started)
                    started = true;

                // Skip over unused elements and soft-deleted elements until we reach
                // a position where the element at rsm.Position is "in use"
                // and not "soft deleted".

                do
                {
                    if(!rsm.Inc())
                        return false;   // Exhausted the search, no more items, stop enumerating.
                }
                while(!elementsInUse[rsm.Position] || softDeleted[rsm.Position]);

                // rsm.Position will now point to an element which is available
                // and not soft-deleted; this is picked up by Current.

                return true;
            }
        }
    }
    public static class HashBucket2Extensions
    {
        public static bool TryStore<T>(this HashBucket2<T> source, int hash, T value)
        {
            return source.TryStore(hash, value, out int _);
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

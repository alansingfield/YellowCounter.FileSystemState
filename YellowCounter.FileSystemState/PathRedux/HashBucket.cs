using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class HashBucketOptions
    {
        public int Capacity { get; set; }
        public int LinearSearchLimit { get; set; }
    }

    /// <summary>
    /// A hash bucket using the Open Addressing scheme. This is a fixed capacity
    /// implementation.
    /// </summary>
    public class HashBucket<T>
    {
        private Memory<T> mem;
        private readonly int capacity;
        private readonly int linearSearchLimit;
        private BitArray elementsInUse;
        private int occupancy;
        private int maxLinearSearch;

        public HashBucket(HashBucketOptions options) : this(options.Capacity, options.LinearSearchLimit) { }

        public HashBucket(int capacity, int linearSearchLimit)
        {
            mem = new T[capacity + linearSearchLimit];
            elementsInUse = new BitArray(capacity);
            occupancy = 0;

            this.capacity = capacity;
            this.linearSearchLimit = linearSearchLimit;
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
        /// Number of slots we are using at the moment
        /// </summary>
        public int Occupancy => this.occupancy;
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
        public virtual bool TryStore(int hash, T value)
        {
            // Calculate which is the first slot we should try
            RSM rsm = new RSM(
                slotFromHash(hash),
                this.capacity,
                this.linearSearchLimit);

            // Starting at the first slot, search for a free slot to put our
            // data into. We might shoot past the end of capacity so
            // using ModuloSlot loops around back to the start.
            while(elementsInUse[rsm.ModuloSlot])
            {
                // Inc() will return false if we have exceeded the search limit.
                if(!rsm.Inc())
                    return false;
            }

            var span = mem.Span;

            // Write to the memory and our "in use" bit array.
            span[rsm.ModuloSlot] = value;
            elementsInUse[rsm.ModuloSlot] = true;

            // If wrapping around we have two copies of the values,
            // one at the normal position and one in the runoff area
            // at the end of the memory buffer.
            // This so we have a contiguous span to slice for the
            // return.
            if(rsm.RawSlot != rsm.ModuloSlot)
            {
                span[rsm.RawSlot] = value;
            }

            // Keep track of our usage.
            occupancy++;

            // Keep track of the longest linear search we've had to do
            // so far.
            if(maxLinearSearch <= rsm.Pos)
                maxLinearSearch = rsm.Pos + 1;

            return true;
        }

        private struct RSM
        {
            private readonly int capacity;
            private readonly int scanLimit;

            public RSM(int baseSlot, int capacity, int scanLimit)
            {
                this.capacity = capacity;
                this.scanLimit = scanLimit;
                this.RawSlot = baseSlot;
                this.ModuloSlot = baseSlot;

                this.Pos = 0;
            }

            public bool Inc()
            {
                Pos++;

                RawSlot++;
                ModuloSlot++;

                if(ModuloSlot >= capacity)
                    ModuloSlot %= capacity;

                return Pos < scanLimit;
            }

            public int RawSlot { get; private set; }
            public int ModuloSlot { get; private set; }
            public int Pos { get; private set; }
        }

        /// <summary>
        /// Modulo divide the hash by our capacity
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private int slotFromHash(int hash) => (int)unchecked((uint)hash % (uint)Capacity);

        /// <summary>
        /// Retrieves a set of possible values for a given hash.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public virtual ReadOnlySpan<T> Retrieve(int hash)
        {
            // Calculate the first spot our result could be in
            int slot = slotFromHash(hash);

            // Search through our usage BitArray from the first slot until
            // we find an unused space. pos will be the last position we found
            // with something in it.
            int pos = 0;
            while(pos < maxLinearSearch)
            {
                int moduloSlot = (slot + pos) % capacity;

                if(!elementsInUse[moduloSlot])
                    break;

                pos++;
            }

            // Return a slice of memory containing a number of candidate values.
            // The caller should then probe these in turn to find the correct one.
            return mem.Span.Slice(slot, pos);
        }

        public ReadOnlySpan<T> AsSpan() => mem.Span;

        public Section RetrieveX(int hash)
        {
            // Calculate the first spot our result could be in
            int slot = slotFromHash(hash);

            return new Section(new Enumerator(
                mem.Span,
                elementsInUse,
                slot,
                maxLinearSearch,
                capacity));
        }

        public ref struct Section
        {
            Enumerator enumerator;

            public Section(Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public Enumerator GetEnumerator() => this.enumerator;
        }

        public Enumerator GetEnumerator()
        {
            var bufSpan = mem.Span;

            return new Enumerator(bufSpan, this.elementsInUse, 0, capacity, capacity);
        }

        public ref struct Enumerator
        {
            private int pos;
            private int length;
            private int capacity;
            private readonly Span<T> bufSpan;
            private readonly BitArray elementsInUse;

            public Enumerator(Span<T> bufSpan, BitArray elementsInUse, int start, int length, int capacity)
            {
                pos = start -1;

                this.bufSpan = bufSpan;
                this.elementsInUse = elementsInUse;
                this.length = length;
                this.capacity = capacity;
            }

            public ref T Current => ref bufSpan[pos];

            public bool MoveNext()
            {
                // BZZZT WRONG -- need to keep iterating until elementsInUse is false.
                // Skip over soft-deleted items
                // Loop round to the start if we reach the end.
                // Keep track of linear search limit
                // Skip over unused items.
                while(!elementsInUse[pos] && pos < bufSpan.Length)
                    pos++;

                if(pos >= bufSpan.Length)
                    return false;

                return true;
            }
        }
    }

}

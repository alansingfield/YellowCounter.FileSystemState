using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace YellowCounter.FileSystemState.PathRedux
{
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
        private int usageCount;
        private int maxLinearSearch;

        public HashBucket(int capacity, int linearSearchLimit)
        {
            mem = new T[capacity + linearSearchLimit];
            elementsInUse = new BitArray(capacity);
            usageCount = 0;

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
        public int UsageCount => this.usageCount;
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
        public bool Store(int hash, T value)
        {
            // Calculate which is the first slot we should try
            int baseSlot = slotFromHash(hash);

            var span = mem.Span;

            // Starting at the first slot, search for a free slot to put our
            // data into.
            for(int pos = 0; pos < linearSearchLimit; pos++)
            {
                // Calculate the slot we are going to try. We might shoot past the
                // end of capacity so we need to wrap around back to the start.
                int rawSlot = baseSlot + pos;
                int moduloSlot = rawSlot % capacity;

                // Skip round until we find a free slot
                if(elementsInUse[moduloSlot])
                    continue;

                // Write to the memory and our "in use" bit array.
                span[moduloSlot] = value;
                elementsInUse[moduloSlot] = true;

                // If wrapping around we have two copies of the values,
                // one at the normal position and one in the runoff area
                // at the end of the memory buffer.
                // This so we have a contiguous span to slice for the
                // return.
                if(rawSlot != moduloSlot)
                {
                    span[rawSlot] = value;
                }

                // Keep track of our usage.
                usageCount++;

                // Keep track of the longest linear search we've had to do
                // so far.
                if(maxLinearSearch <= pos)
                    maxLinearSearch = pos + 1;

                return true;
            }

            // Went past the linearSearchLimit, not enough slots to store
            return false;
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
        public ReadOnlySpan<T> Retrieve(int hash)
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

    }
}

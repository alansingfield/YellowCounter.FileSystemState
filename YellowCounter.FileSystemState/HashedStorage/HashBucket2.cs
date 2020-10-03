using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState.HashedStorage
{
    /// <summary>
    /// A hash bucket using the Open Addressing scheme. This is a fixed capacity
    /// implementation.
    /// </summary>
    public partial class HashBucket2<T>
    {
        private T[] mem;
        private readonly int capacity;
        private readonly int linearSearchLimit;
        private readonly BitArray elementsInUse;
        private readonly BitArray softDeleted;
        private readonly int sizeofT;
        private int occupancy;
        private int usage;

        private int numChunks;
        private int[] chunkProbeDepth;

        /// <summary>
        /// Creates a hash bucket using Open Addresssing.
        /// </summary>
        /// <param name="capacity">fixed maximum number of items we can store</param>
        /// <param name="linearSearchLimit">Maximum distance from the hash provided to the
        /// actual position we've stored the item at</param>
        public HashBucket2(HashBucket2Options options)
        {
            this.capacity = options.Capacity;
           // this.linearSearchLimit = options.LinearSearchLimit;

            mem = new T[capacity];
            elementsInUse = new BitArray(capacity);
            softDeleted = new BitArray(capacity);
            occupancy = 0;
            usage = 0;

            this.sizeofT = Unsafe.SizeOf<T>();

            int chunkSize = options.ChunkSize;
            if(chunkSize < 1)
                chunkSize = 1;

            numChunks = 1 + (capacity / chunkSize);

            this.chunkProbeDepth = new int[numChunks];
        }

        /// <summary>
        /// Overall number of slots
        /// </summary>
        public int Capacity => this.capacity;
        /// <summary>
        /// Number of slots we are using at the moment (including soft deleted)
        /// </summary>
        public int Occupancy => this.occupancy;
        /// <summary>
        /// Number of used slots excluding soft-deleted
        /// </summary>
        public int Usage => this.usage;

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
            int slot = slotFromHash(hash);

            // For a given block of array positions, we store the maximum known
            // probe depth. This avoids searching the whole array.
            ref var probeDepth = ref probeDepthFromSlot(slot);

            // MULTITHREADING - probeDepth might need to go further than +1 to account for
            // other threads accessing same chunk.

            // Calculate which is the first slot we should try
            var cursor = new Cursor(
                slot,
                this.capacity,
                probeDepth + 1);

            bool foundSlot = false;

            // Starting at the first slot, search for a free slot to put our
            // data into. We might shoot past the end of the array so
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
            // so far for this chunk. Never let this get bigger than the capacity
            // of the array as we would end up in an infinite loop.
            if(probeDepth <= cursor.MoveCount && probeDepth <= this.capacity)
            {
                probeDepth = cursor.MoveCount + 1;
            }

            // Return the position we stored the item at.
            index = cursor.Index;
            return true;
        }

        /// <summary>
        /// Soft-delete the item at the given index position. The item will
        /// remain in the internal array but not be enumerated any more or
        /// accessible through []
        /// </summary>
        /// <param name="index"></param>
        public void DeleteAt(int index)
        {
            // We will be in range if item is a member of the mem array.
            if(index < 0 || index >= softDeleted.Length)
                throw new IndexOutOfRangeException();

            deleteAtInternal(index);
        }

        /// <summary>
        /// Soft-delete the specified item. The item will remain in the internal
        /// array. If you continue to use the ref to this item after deleting, you
        /// must ensure that you do not call TryStore() as this could write a new
        /// item in that slot.
        /// </summary>
        /// <param name="item"></param>
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


        private void deleteAtInternal(int position)
        {
            if(!softDeleted[position])
            {
                softDeleted[position] = true;
                this.usage--;
            }
        }

        /// <summary>
        /// Modulo divide the hash by our capacity
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private int slotFromHash(int hash) => (int)unchecked((uint)hash % (uint)Capacity);

        private int chunkFromSlot(int position) => position / numChunks;

        private ref int probeDepthFromSlot(int position) => ref this.chunkProbeDepth[chunkFromSlot(position)];

        /// <summary>
        /// Enumerate items stored under the given hash. Note that
        /// due to hash collisions, you will also be presented with items which do
        /// NOT match the hash. It is your responsibility to ignore these.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public Segment Retrieve(int hash)
        {
            // Calculate the first spot our result could be in
            int slot = slotFromHash(hash);

            // Calculate how far the maximum search depth is
            int probeDepth = probeDepthFromSlot(slot);

            // Enumerate from this slot onwards.
            return new Segment(new Enumerator(
                mem,
                elementsInUse,
                softDeleted,
                slot,
                probeDepth,
                capacity,
                contiguous: true));
        }

        /// <summary>
        /// Access an item by its index position. An error will be raised if there is
        /// no item at the specified spot. Since this is a "ref" you can use this in
        /// the left hand side of an assignment to amend the item.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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

        /// <summary>
        /// This enumerates through every available, non-soft-deleted item
        /// in the set.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
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
                contiguous: false); // Stop when we hit the first gap.
        }
    }
}

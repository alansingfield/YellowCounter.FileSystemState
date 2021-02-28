using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using YellowCounter.FileSystemState.Bits;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState.HashedStorage
{
    /// <summary>
    /// A hash bucket using the Open Addressing scheme. This is a fixed capacity
    /// implementation.
    /// </summary>
    public partial class HashBucket<T> : IDisposable
    {
        private T[] mem;
        private readonly int capacity;
        private readonly BitArray64 elementsInUse;
        private readonly BitArray64 softDeleted;
        private int occupancy;
        private int usage;
        private readonly int chunkSize;


        private int numChunks;
        private int[] chunkProbeDepth;
        private bool disposedValue;

        /// <summary>
        /// Creates a hash bucket using Open Addresssing. This is a fixed size
        /// implementation. 
        /// </summary>
        /// <param name="options">Sizing options</param>
        public HashBucket(HashBucketOptions options)
        {
            this.mem = new T[options.Capacity];

            this.capacity = options.Capacity;
            this.elementsInUse = new BitArray64(this.Capacity);
            this.softDeleted = new BitArray64(this.Capacity);

            this.occupancy = 0;
            this.usage = 0;

            this.chunkSize = options.ChunkSize;

            // If you specify 0 for the ChunkSize then put everything in one chunk.
            if(this.chunkSize < 1)
                this.chunkSize = capacity;

            this.numChunks = this.capacity / this.chunkSize;

            this.chunkProbeDepth = new int[this.numChunks + 1];
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

        public int ChunkSize => chunkSize;

        /// <summary>
        /// We use two pointers within the search space to reduce the occurrence of
        /// hash clustering. This function converts a hash code (which gives the first
        /// position) to another hash code (for the second position). By default this
        /// uses a non-repeating pseudo-random sequence from PseudoRandomSequence
        /// but it can be overridden using the HashBucketOptions.Permute function.
        /// Must be deterministic.
        /// </summary>
        /// <param name="hash">Input hash</param>
        /// <returns>Secondary hash corresponding to the input</returns>
        protected virtual int Permute(int hash)
        {
            // Use the non-repeating pseudo-random sequence.
            return PseudoRandomSequence.Permute(hash);
        }

        /// <summary>
        /// Stores value against the specified hash. Multiple values can be stored
        /// against the same hash (it does not overwrite).
        /// 
        /// DOES NOT RAISE AN EXCEPTION if it can't store the value.
        /// You must check the return value.
        /// </summary>
        /// <param name="hash">Hashcode to store</param>
        /// <param name="value">Value to store</param>
        /// <param name="index">Outputs position of where value was stored. Will be set to -1 if store fails.</param>
        /// <returns>True if storing worked.</returns>
        public virtual bool TryStore(int hash, T value, out int index)
        {
            // Are we completely full? Don't try to store anything more.
            if(usage == capacity)
            {
                index = -1;
                return false;
            }

            // Calculate position to search from modulo of hash.
            int slotA = slotFromHash(hash);

            // To avoid hash clustering, we also calculate a secondary position to
            // search using a pseudo-random function of the hash.
            int slotB = slotFromHash(Permute(hash));

            // For a given block of array positions, we store the maximum known
            // probe depth. This avoids searching the whole array.
            ref var probeDepth = ref probeDepthFromSlot(slotA);

            // Create a cursor to search from slot A, then slot B, then slot A
            // again in turn.
            var cursor = new DualCursor(
                this.capacity,
                slotA,
                slotB,
                this.capacity);

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

            // This shouldn't happen because we check that we have not exceeded
            // our usage earlier on, but leave it in for now. It might be caused
            // by re-entrancy, perhaps?
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
            if(cursor.MoveCount > probeDepth)
            {
                // Note: This is a ref variable so we are actually writing to
                // this.chunkProbeDepth[n]
                probeDepth = cursor.MoveCount;
            }

            // Return the position we stored the item at.
            index = cursor.Index;
            return true;
        }

        /// <summary>
        /// Soft-delete the item at the given index position. The item will
        /// remain in the internal array but not be enumerated any more or
        /// accessible through []. Any refs you have to this item will remain
        /// valid until your next call to TryStore() when the position might
        /// be used again.
        /// </summary>
        /// <param name="index">Position from 0..capacity-1</param>
        public void DeleteAt(int index)
        {
            // We will be in range if item is a member of the mem array.
            if(index < 0 || index >= softDeleted.Length)
                throw new IndexOutOfRangeException();

            deleteAtInternal(index);
        }

        private void deleteAtInternal(int index)
        {
            if(!softDeleted[index])
            {
                softDeleted[index] = true;
                this.usage--;
            }
        }

        /// <summary>
        /// Modulo divide the hash by our capacity
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private int slotFromHash(int hash) => (int)unchecked((uint)hash % (uint)Capacity);

        private int chunkFromSlot(int position) => position / ChunkSize;

        private ref int probeDepthFromSlot(int position) => ref this.chunkProbeDepth[chunkFromSlot(position)];

        /// <summary>
        /// Returns the maximum possible probe depth for a given hash value.
        /// We store the maximum probe depth for each "chunk" of hash values.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public int ProbeDepth(int hash) => probeDepthFromSlot(slotFromHash(hash));

        public double AverageProbeDepth()
        {
            return this.chunkProbeDepth.Average();
        }

        /// <summary>
        /// Enumerate items stored under the given hash. Note that
        /// due to hash collisions, you will also be presented with items which do
        /// NOT match the hash. It is your responsibility to ignore these.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public Segment Retrieve(int hash)
        {
            // Calculate the first slot our result could be in.
            int slotA = slotFromHash(hash);

            // Run the permutation function to determine the second slot
            int slotB = slotFromHash(Permute(hash));

            // Calculate how far the maximum search depth is
            int probeDepth = probeDepthFromSlot(slotA);

            // Enumerate from this slot onwards.
            return new Segment(new DualEnumerator(
                mem,
                elementsInUse,
                softDeleted,
                slotA,
                slotB,
                probeDepth,
                capacity));
        }

        /// <summary>
        /// Enumerate items stored under the given hash. Note that
        /// due to hash collisions, you will also be presented with items which do
        /// NOT match the hash. It is your responsibility to ignore these.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public DualIndexSegment RetrieveIndices(int hash)
        {
            // Calculate the first slot our result could be in.
            int slotA = slotFromHash(hash);

            // Run the permutation function to determine the second slot
            int slotB = slotFromHash(Permute(hash));

            // Calculate how far the maximum search depth is
            int probeDepth = probeDepthFromSlot(slotA);

            // Enumerate from this slot onwards.
            return new DualIndexSegment(new DualIndexEnumerator(
                elementsInUse,
                softDeleted,
                slotA,
                slotB,
                probeDepth,
                capacity));
        }

        /// <summary>
        /// Access an item by its index position. An error will be raised if there is
        /// no item at the specified spot. Since this is a "ref" you can use this in
        /// the left hand side of an assignment to amend the item.
        /// </summary>
        /// <param name="index">Index position from 0..capacity-1</param>
        /// <returns>Reference to the item</returns>
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
        /// in the set in positional order. This is a reference enumerator
        /// so the items can be modified in-situ.
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
                scanLimit,          // All the elements
                capacity);          // Enumerate to the end.
        }

        public IndexSegment AllIndices()
        {
            int scanLimit = this.occupancy == 0 ? 0 : this.capacity;

            return new IndexSegment(new IndexEnumerator(
                this.elementsInUse,
                this.softDeleted,
                0,                  // Start at the beginning
                scanLimit,          // All the elements
                capacity));         // Enumerate to the end.
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    elementsInUse?.Dispose();
                    softDeleted?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HashBucket()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

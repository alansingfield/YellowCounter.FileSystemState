using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Numerics;

namespace YellowCounter.FileSystemState.Bits
{
    public class BitArray64 : IDisposable
    {
        private int length;
        private readonly ArrayPool<ulong> arrayPool;
        private ulong[] array;
        private bool disposedValue;

        public BitArray64(int length) : this(length, false, ArrayPool<ulong>.Shared) { }
        public BitArray64(int length, bool initialValue) : this(length, initialValue, ArrayPool<ulong>.Shared) { }
        public BitArray64(int length, ArrayPool<ulong> arrayPool) : this(length, false, ArrayPool<ulong>.Shared) { }

        public BitArray64(int length, bool initialValue, ArrayPool<ulong> arrayPool)
        {
            this.length = length;
            this.arrayPool = arrayPool;
            
            this.array = allocate(arrayLength(length));

            // Note that array could contain leftover data from previous work
            // so we have to clear it.
            SetAll(initialValue);
        }

        private ulong[] allocate(int intCount)
        {
            return arrayPool.Rent(intCount);
        }

        private void switchArrayTo(ulong[] newArray)
        {
            if(this.array != null)
            {
                arrayPool.Return(this.array);
            }
            this.array = newArray;
        }
        
        private void deallocate()
        {
            arrayPool.Return(this.array);
            this.array = null;
        }

        private int actualLength => this.array.Length;

        public int Length
        {
            get => this.length;
        }

        public void Resize(int newSize, bool forceReallocation = false)
        {
            int newLongLength = arrayLength(newSize);

            if(forceReallocation 
                || newLongLength > actualLength 
                || newLongLength < actualLength - 256)
            {
                ulong[] newArray = allocate(newLongLength);

                Array.Copy(this.array, newArray,
                    newLongLength > this.array.Length
                        ? this.array.Length
                        : newLongLength);

                switchArrayTo(newArray);

                if(newSize > this.length)
                {
                    // Clear out the relevant bits of the final ulong
                    int last = arrayLength(this.length) - 1;
                    int bits = this.length % 64;
                    if(bits > 0)
                        this.array[last] &= (1ul << bits) - 1;

                    // Clear remaining values
                    Array.Clear(this.array, last + 1, newLongLength - last - 1);
                }
            }

            this.length = newSize;
        }

        public int Count => this.length;

        public bool this[int index]
        {
            get
            {
                if(index < 0 || index >= this.length)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be in range 0..length-1");

                return (this.array[index >> 6] & (1ul << (index % 64))) != 0;
            }
            set
            {
                if(index < 0 || index >= this.length)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be in range 0..length-1");

                if(value)
                {
                    this.array[index >> 6] |= (1ul << (index % 64));
                }
                else
                {
                    this.array[index >> 6] &= ~(1ul << (index % 64));
                }
            }
        }


        public void SetAll(bool value)
        {
            if(!value)
            {
                Array.Clear(array, 0, array.Length);
            }
            else
            {
                int longLength = arrayLength(this.length);
                for(int i = 0; i < longLength; i++)
                {
                    array[i] = ulong.MaxValue;
                }
            }
        }

        //public int IndexOf(bool value, int startIndex)
        //{
        //    value = true;

        //    int startUlong = startIndex >> 6;
        //    int lastUlong = arrayLength(this.length) - 1;

        //    int startBit = startIndex % 64;

        //    //uint mask = 1u << (startIndex % 32);

        //    //if(!value)
        //    //    mask = ~mask;

        //    for(int ulPos = startUlong; ulPos <= lastUlong; ulPos++)
        //    {
        //        ulong data64 = this.array[ulPos];

        //        ulong mask = 1ul;

        //        // If on the 0th bit we can compare all 64 bits in one
        //        // go, if all zeros, there's no 1 to be found.
        //        if(startBit == 0)
        //        {
        //            if(data64 == 0ul)
        //                continue;       // No point in bit-by-bit comparison

        //            // Mask will be 0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0001
        //        }
        //        else
        //        {
        //            mask <<= (startBit % 64);
        //        }

        //        // Normally we look through to the 64th bit; but if on the last
        //        // ulong we should only go to the last relevant one.
        //        int endBit = 63;
        //        if(ulPos == lastUlong)
        //        {
        //            endBit = ((this.length - 1) % 64);
        //        }

        //        // Bit-by-bit comparison.
        //        for(int bit = startBit; bit <= endBit; bit++)
        //        {
        //            if((data64 & mask) != 0ul)
        //                return (ulPos << 6) + bit;

        //            mask <<= 1;     // Shift mask left by 1 bit each time.
        //        }

        //        // Once we've compared the first 64 bits startBit will be zero
        //        // from this point forwards.
        //        startBit = 0;
        //    }

        //    return -1;
        //}

        private int arrayLength(int bitCount) => 
            bitCount <= 0 
            ? 0
            : ((bitCount - 1) >> 6) + 1;

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    deallocate();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

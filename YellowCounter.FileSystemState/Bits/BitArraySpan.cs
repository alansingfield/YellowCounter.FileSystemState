using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Bits
{
    public ref struct BitArraySpan
    {
        private readonly BitArray array;
        private readonly int start;
        private readonly int length;

        public BitArraySpan(BitArray array) : this(array, 0, array.Length) {}
        public BitArraySpan(BitArray array, int start) : this(array, start, array.Length - start) { }

        public BitArraySpan(BitArray array, int start, int length)
        {
            if(start < 0)
                throw new ArgumentException(nameof(start));

            if(length < 0)
                throw new ArgumentException(nameof(length));

            if(start + length > array.Length)
                throw new ArgumentException(nameof(length));

            this.array = array;
            this.start = start;
            this.length = length;
        }

        public bool this[int index]
        {
            get
            {
                verifyIndex(index);

                return array[index + start];
            }
            set
            {
                verifyIndex(index);

                array[index + start] = value;
            }
        }

        private void verifyIndex(int index)
        {
            if(index < 0 || index >= length)
                throw new IndexOutOfRangeException();
        }
    }

    public static class BitArrayExtensions
    {
        public static BitArraySpan ToSpan(this BitArray bitArray) => new BitArraySpan(bitArray);
        public static BitArraySpan ToSpan(this BitArray bitArray, int start) => new BitArraySpan(bitArray, start);
        public static BitArraySpan ToSpan(this BitArray bitArray, int start, int length) => new BitArraySpan(bitArray, start, length);
    }
}

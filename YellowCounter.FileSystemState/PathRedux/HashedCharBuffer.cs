﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class HashedCharBuffer
    {
        private readonly int linearSearchLimit;
        private CharBuffer charBuffer;
        private HashBucket chainedLookup;
        private readonly Func<IHashCode> newHashCode;

        public HashedCharBuffer(HashedCharBufferOptions options)
        {
            charBuffer = new CharBuffer(options.InitialCharCapacity);
            chainedLookup = new HashBucket(options.InitialHashCapacity, options.LinearSearchLimit);

            this.newHashCode = options.NewHashCode;
            this.linearSearchLimit = options.LinearSearchLimit;
        }

        public int LinearSearchLimit => this.linearSearchLimit;
        public int CharCapacity => charBuffer.Capacity;
        public int HashCapacity => chainedLookup.Capacity;

        /// <summary>
        /// Returns index position
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int Store(ReadOnlySpan<char> text)
        {
            int hash = hashSequence(text);
            int foundPos = findByHash(hash, text);

            if(foundPos != -1)
                return foundPos;

            int pos = charBuffer.Store(text);
            if(pos == -1)
            {
                int newSize = charBuffer.Capacity * 2;
                if(newSize < text.Length + charBuffer.Capacity + 2) // Allow 2 for null terminators
                    newSize = charBuffer.Capacity + text.Length + 2;

                charBuffer.Resize(newSize);

                pos = charBuffer.Store(text);
                if(pos == -1)
                    throw new Exception("Resizing charBuffer didn't give us enough space");
            }

            if(!chainedLookup.Store(hash, pos))
            {
                rebuildLookup();
                chainedLookup.Store(hash, pos);
            }

            return pos;
        }

        public ReadOnlySpan<char> Retrieve(int pos) => charBuffer.Retrieve(pos);
        public ReadOnlySequence<char> Retrieve(IEnumerable<int> indices) => charBuffer.Retrieve(indices);

        public string CreateString(IEnumerable<int> indices) => charBuffer.CreateString(indices);

        public int Find(ReadOnlySpan<char> text)
        {
            int hash = hashSequence(text);
            return findByHash(hash, text);
        }

        private int findByHash(int hash, ReadOnlySpan<char> text)
        {
            var indices = chainedLookup.Retrieve(hash);
            return charBuffer.Match(text, indices);
        }

        private int hashSequence(ReadOnlySpan<char> text) => newHashCode().HashSequence(text);
        
        private void rebuildLookup()
        {
            // Doubling capacity will halve the number of moduloed hash collisions
            var newLookup = new HashBucket(chainedLookup.Capacity * 2, linearSearchLimit);

            // Populate a new lookup from our existing data.
            foreach(var itm in charBuffer)
            {
                if(!newLookup.Store(hashSequence(itm.Span), itm.Pos))
                    throw new Exception($"Too many hash collisions. Increase {nameof(LinearSearchLimit)} to overcome.");
            }

            // Use the new lookup
            chainedLookup = newLookup;
        }
    }
}





// Split by backslash / slash

// Starting at the longest sequence,
// e.g. C:\abc\cde\efg\ghi\
// then going backwards as
// C:\abc\cde\efg\
// C:\abc\cde\
// C:\abc\

// Generate the hashcode of the text.
// Look up the hashcode in the dictionary
// If we found it, we will get two things:
// Index of the tail entry
// Index of the parent

// Create a new record
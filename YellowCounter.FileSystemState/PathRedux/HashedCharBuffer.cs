using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class HashedCharBuffer
    {
        private CharBuffer charBuffer;
        private HashBucket<int> hashLookup;
        private HashedCharBufferOptions options;

        public HashedCharBuffer(HashedCharBufferOptions options = null)
        {
            this.options = (options == null) ?
                new HashedCharBufferOptions()
                : options.Clone();

            charBuffer = new CharBuffer(options.InitialCharCapacity);
            hashLookup = new HashBucket<int>(options.HashBucketOptions);
        }

        public int CharCapacity => charBuffer.Capacity;
        public int HashCapacity => hashLookup.Capacity;

        /// <summary>
        /// Returns index position
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int Store(ReadOnlySpan<char> text)
        {
            int hash = HashSequence(text);
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

                // this would be a maths error in not calculating the new length properly.
                if(pos == -1)
                    throw new Exception("Resizing charBuffer didn't give us enough space");
            }

            storeHashInLookup(hash, pos);

            return pos;
        }



        public ReadOnlySpan<char> Retrieve(int pos) => charBuffer.Retrieve(pos);
        public ReadOnlySequence<char> Retrieve(IEnumerable<int> indices) => charBuffer.Retrieve(indices);

        public int Find(ReadOnlySpan<char> text)
        {
            int hash = HashSequence(text);
            return findByHash(hash, text);
        }

        private int findByHash(int hash, ReadOnlySpan<char> text)
        {
            foreach(var index in hashLookup.Retrieve(hash))
            {
                if(charBuffer.Match(text, index))
                    return index;
            }

            return -1;
        }

        public int HashSequence(ReadOnlySpan<char> text) => options.NewHashCode().HashSequence(text);

        private void storeHashInLookup(int hash, int pos)
        {
            resizeIfNeeded(1);

            if(!hashLookup.TryStore(hash, pos))
                throw new Exception("Unable to store in lookup");
        }

        private void resizeIfNeeded(int headroom)
        {
            int? newCapacity = options.SizePolicy.MustResize(
                hashLookup.Usage + headroom,
                hashLookup.Capacity);

            if(newCapacity != null)
                rebuildLookup(newCapacity.Value);
        }

        private void rebuildLookup(int newCapacity)
        {
            var opts = this.options.HashBucketOptions.Clone();
            opts.Capacity = newCapacity;

            // Create a replacement hash bucket of the new size
            var replacement = new HashBucket<int>(opts);

            // Populate the replacement with our existing data
            foreach(var itm in charBuffer)
            {
                if(!replacement.TryStore(HashSequence(itm.Span), itm.Pos))
                    throw new Exception("Unable to store data after resizing to fit");
            }

            // We've managed to store everything, REPLACE the old lookup with a new one.
            this.hashLookup = replacement;
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
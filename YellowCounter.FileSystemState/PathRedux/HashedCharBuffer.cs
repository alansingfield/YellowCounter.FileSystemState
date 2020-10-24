using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class HashedCharBuffer
    {
        private CharBuffer charBuffer;
        private HashBucket<int> hashLookup;
        private readonly Func<IHashCode> newHashCode;

        public HashedCharBuffer(HashedCharBufferOptions options)
        {
            charBuffer = new CharBuffer(options.InitialCharCapacity);
            hashLookup = new HashBucket<int>(new HashBucketOptions()
            {
                Capacity = options.InitialHashCapacity,
                ChunkSize = 32,
            });

            this.newHashCode = options.NewHashCode;
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

                // this would be a maths error in not calculating the new length properly.
                if(pos == -1)
                    throw new Exception("Resizing charBuffer didn't give us enough space");
            }

            storeHashInLookup(hash, pos);

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
            foreach(var index in hashLookup.Retrieve(hash))
            {
                var position = charBuffer.Match(text, index);

                if(position != -1)
                    return position;
            }

            return -1;
        }

        private int hashSequence(ReadOnlySpan<char> text) => newHashCode().HashSequence(text);


        private void storeHashInLookup(int hash, int pos)
        {
            if(((float)hashLookup.Usage / hashLookup.Capacity) < 0.8f)
            {
                if(hashLookup.TryStore(hash, pos))
                    return;
            }
         
            rebuildLookup();

            if(!hashLookup.TryStore(hash, pos))
                throw new Exception("Unable to store in lookup");
        }

        private void rebuildLookup()
        {
            // Given the headroom we need, get a list of possible sizes to try for the lookup.
            foreach(var opts in hashLookup.SizeOptions(headroom: 1))
            {
                // Create a replacement hash bucket of the new size
                var replacement = new HashBucket<int>(opts);

                // Populate the replacement with our existing data
                foreach(var itm in charBuffer)
                {
                    if(!replacement.TryStore(hashSequence(itm.Span), itm.Pos))
                        continue;   // Can't store in the replacement, try a different size
                }

                // We've managed to store everything, REPLACE the old lookup with a new one.
                this.hashLookup = replacement;
                return;
            }

            // Theoretically this shouldn't happen, but...
            // We've got a backstop which increases both the capacity and the linear search
            // limit - what more could we do? Let's find out...
            throw new Exception("Too many hash collisions.");
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
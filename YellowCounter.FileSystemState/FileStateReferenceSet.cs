using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState
{
    internal class FileStateReferenceSet : ReferenceSet<(int, int), FileState>
    {
        private readonly Func<(int, int), int> hashFunction;

        public FileStateReferenceSet(FileStateReferenceSetOptions options) : base(options)
        {
            this.hashFunction = options.HashFunction;
        }

        protected override int GetHashOfKey((int, int) key)
        {
            // For testing we can override the hash function.
            if(hashFunction != null)
                return hashFunction(key);

            // Since the values of item1 / item2 are in similar ranges, bit shift them
            // around so there are less hash collisions.
            return ((key.Item1 << 16) | (key.Item1 >> 16))
                ^ ((key.Item2 << 8) | (key.Item2 >> 24));
        }

        protected override (int, int) GetKey(FileState item)
        {
            return (item.DirectoryRef, item.FilenameRef);
        }

        protected override bool Match(FileState item, (int, int) key)
        {
            return (item.DirectoryRef, item.FilenameRef) == key;

        }
    }

    internal class FileStateReferenceSetOptions : ReferenceSetOptions
    {
        public Func<(int, int), int> HashFunction { get; set; }
    }
}

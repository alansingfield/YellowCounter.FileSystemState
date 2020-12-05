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
            // For testing we override the hashfunction with a deterministic one.
            // For real usage we want the .NET one.
            if(hashFunction != null)
                return hashFunction(key);
            
            // The two values (directory idx / file idx) are in similar ranges so shift
            // the directory by 16 bits so we clash every 65536

            return ((key.Item1 << 16) | (key.Item1 >> 16))
                ^ ((key.Item2 << 8) | (key.Item2 >> 24));

            return key.GetHashCode();
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

    internal class FileStateReferenceSetOptions : SetByRefOptions
    {
        public Func<(int, int), int> HashFunction { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState
{
    internal class FileStateReferenceSet : ReferenceSet<(int, int), FileState>
    {
        public FileStateReferenceSet(FileStateReferenceSetOptions options) : base(options.ReferenceSetOptions)
        {

        }

        protected override int GetHashOfKey((int, int) key)
        {
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

}

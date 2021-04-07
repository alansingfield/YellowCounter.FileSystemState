using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket<T>
    {
        public ref struct Enumerator
        {
            private IndexEnumerator indexEnumerator;
            private readonly T[] mem;

            public Enumerator(
                T[] mem,
                BitArray elementsInUse,
                BitArray softDeleted,
                int start,
                int scanLimit,
                int capacity)
            {
                this.mem = mem;

                indexEnumerator = new IndexEnumerator(
                    elementsInUse,
                    softDeleted,
                    start,
                    scanLimit,
                    capacity
                );
            }

            public ref T Current
            {
                get
                {
                    return ref mem[indexEnumerator.Current];
                }
            }

            public bool MoveNext()
            {
                return indexEnumerator.MoveNext();
            }
        }
    }
}

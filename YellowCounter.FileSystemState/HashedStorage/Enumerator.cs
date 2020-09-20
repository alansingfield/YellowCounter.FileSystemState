using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket2<T>
    {
        public ref struct Enumerator
        {
            private Cursor cursor;

            private readonly T[] mem;
            private readonly BitArray elementsInUse;
            private readonly BitArray softDeleted;
            private bool contiguous;

            public Enumerator(
                T[] mem,
                BitArray elementsInUse,
                BitArray softDeleted,
                int start,
                int scanLimit,
                int capacity,
                bool contiguous)
            {
                this.mem = mem;
                this.elementsInUse = elementsInUse;
                this.softDeleted = softDeleted;
                this.contiguous = contiguous;

                this.cursor = new Cursor(
                    start,
                    capacity,
                    scanLimit);
            }

            public ref T Current
            {
                get
                {
                    if(!cursor.Started)
                        throw new InvalidOperationException();

                    return ref mem[cursor.Index];
                }
            }

            public bool MoveNext()
            {
                while(cursor.MoveNext())
                {
                    // The cursor can be used in two ways. If contiguous, we must have
                    // a continuous sequence of elements all of which are "in use".
                    // The enumeration will stop as soon as we hit an element which has
                    // never been used.
                    // If non-contiguous, we skip over elements which are not "in use"
                    // and carry on until we find an "in use" element.
                    if(!elementsInUse[cursor.Index])
                    {
                        if(contiguous)
                            return false;
                        else
                            continue;
                    }

                    // Skip over soft deleted items.
                    if(softDeleted[cursor.Index])
                        continue;

                    // Element is "in use" and not "soft deleted" so return it.
                    return true;
                }

                // Exhausted the maximum search length of the cursor, stop enumerating.
                return false;
            }
        }
    }
}

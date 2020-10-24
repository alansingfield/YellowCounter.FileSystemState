using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Bits;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket<T>
    {
        public ref struct DualEnumerator
        {
            private DualCursor cursor;

            private readonly T[] mem;
            private readonly BitArray64 elementsInUse;
            private readonly BitArray64 softDeleted;

            public DualEnumerator(
                T[] mem,
                BitArray64 elementsInUse,
                BitArray64 softDeleted,
                int startIndexA,
                int startIndexB,
                int scanLimit,
                int capacity)
            {
                this.mem = mem;
                this.elementsInUse = elementsInUse;
                this.softDeleted = softDeleted;

                this.cursor = new DualCursor(
                    capacity,
                    startIndexA,
                    startIndexB,
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
                    // We must have a continuous sequence of elements all of which are "in use".
                    // The enumeration will stop as soon as we hit an element which has
                    // never been used.
                    if(!elementsInUse[cursor.Index])
                        return false;

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

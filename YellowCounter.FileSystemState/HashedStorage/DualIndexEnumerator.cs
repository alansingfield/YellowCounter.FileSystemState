using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket<T>
    {
        public ref struct DualIndexEnumerator
        {
            private DualCursor cursor;

            private readonly BitArray elementsInUse;
            private readonly BitArray softDeleted;

            public DualIndexEnumerator(
                BitArray elementsInUse,
                BitArray softDeleted,
                int startIndexA,
                int startIndexB,
                int scanLimit,
                int capacity)
            {
                this.elementsInUse = elementsInUse;
                this.softDeleted = softDeleted;

                this.cursor = new DualCursor(
                    capacity,
                    startIndexA,
                    startIndexB,
                    scanLimit);
            }

            public int Current
            {
                get
                {
                    if(!cursor.Started)
                        throw new InvalidOperationException();

                    return cursor.Index;
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

        public ref struct DualIndexSegment
        {
            DualIndexEnumerator enumerator;

            public DualIndexSegment(DualIndexEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public DualIndexEnumerator GetEnumerator() => this.enumerator;
        }
    }
}

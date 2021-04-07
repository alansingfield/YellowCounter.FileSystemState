using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket<T>
    {
        public ref struct IndexEnumerator
        {
            private Cursor cursor;

            private readonly BitArray elementsInUse;
            private readonly BitArray softDeleted;

            public IndexEnumerator(
                BitArray elementsInUse,
                BitArray softDeleted,
                int start,
                int scanLimit,
                int capacity)
            {
                this.elementsInUse = elementsInUse;
                this.softDeleted = softDeleted;

                this.cursor = new Cursor(
                    start,
                    capacity,
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
                    // We skip over elements which are not "in use"
                    // and carry on until we find an "in use" element.
                    if(!elementsInUse[cursor.Index])
                        continue;

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

        public ref struct IndexSegment
        {
            IndexEnumerator enumerator;

            public IndexSegment(IndexEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public IndexEnumerator GetEnumerator() => this.enumerator;
        }
    }
}

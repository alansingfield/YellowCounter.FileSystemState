using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket<T>
    {
        public ref struct Segment
        {
            DualEnumerator enumerator;

            public Segment(DualEnumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public DualEnumerator GetEnumerator() => this.enumerator;
        }
    }
}

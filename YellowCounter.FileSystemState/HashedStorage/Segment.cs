using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket2<T>
    {
        public ref struct Segment
        {
            Enumerator enumerator;

            public Segment(Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public Enumerator GetEnumerator() => this.enumerator;
        }
    }
}

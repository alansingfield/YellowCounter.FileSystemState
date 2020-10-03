using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket2<T>
    {
        // Similar to double-hashing but we just use two sections of data and
        // search in an A/B/A/B fashion.
        // This allows us to keep cache locality but avoid clustering since
        // (hashA % capacity) will mostly be different to (hashB % capacity)
        // hashB is derived from hashA using a non-repeating pseudorandom sequence.

        public struct DualCursor
        {
            private bool isA;

            private Cursor cursorA;
            private Cursor cursorB;

            public DualCursor(int capacity,
                              int startIndexA,
                              int startIndexB,
                              int probeLimitA,
                              int probeLimitB)
            {
                this.Capacity = capacity;
                this.StartIndexA = startIndexA;
                this.StartIndexB = startIndexB;

                this.isA = true;

                int maxProbeA;
                int maxProbeB;

                // When indexA is near to but smaller than indexB, the A cursor
                // could end up straying into cursor B's range. So limit the
                // probe depth of A. Same applies in reverse if indexB < indexA.
                // If they are precisely the same then prefer cursorA. cursorB
                // will not be used at all.
                if(startIndexA <= startIndexB)
                {
                    maxProbeA = startIndexB - startIndexA;
                    maxProbeB = capacity - maxProbeA;
                }
                else
                {
                    maxProbeB = startIndexA - startIndexB;
                    maxProbeA = capacity - maxProbeB;
                }

                if(probeLimitA > maxProbeA)
                    probeLimitA = maxProbeA;

                if(probeLimitB > maxProbeB)
                    probeLimitB = maxProbeB;

                this.cursorA = new Cursor(startIndexA, capacity, probeLimitA);
                this.cursorB = new Cursor(startIndexB, capacity, probeLimitB);
            }

            public bool MoveNext()
            {
                if(isA && !cursorA.Ended)
                {
                    bool result = cursorA.MoveNext();

                    if(!cursorB.Ended)
                        isA = false;

                    return result;
                }
                else
                {
                    if(cursorB.Ended)
                        return false;

                    bool result = cursorB.MoveNext();

                    if(!cursorA.Ended)
                        isA = true;

                    return result;
                }
            }

            public int Index => this.isA ? cursorA.Index : cursorB.Index;

            public int Capacity { get; }
            public int StartIndexA { get; }
            public int StartIndexB { get; }
            public int ProbeLimitA => cursorA.MoveLimit;
            public int ProbeLimitB => cursorB.MoveLimit;


        }
    }
}

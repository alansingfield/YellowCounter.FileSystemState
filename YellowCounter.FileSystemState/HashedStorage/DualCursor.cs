using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
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
                            int probeLimit)
        {
            if(startIndexA < 0 || startIndexA >= capacity)
                throw new ArgumentException(
                    $"{nameof(startIndexA)} must be between 0 and ${nameof(capacity)}-1",
                    nameof(startIndexA));

            if(startIndexB < 0 || startIndexB >= capacity)
                throw new ArgumentException(
                    $"{nameof(startIndexB)} must be between 0 and ${nameof(capacity)}-1",
                    nameof(startIndexB));

            if(probeLimit < 0 || probeLimit > capacity)
                throw new ArgumentException(
                    $"{nameof(probeLimit)} must be between 0 and ${nameof(capacity)}",
                    nameof(probeLimit));

            if(capacity < 0)
                throw new ArgumentException(
                    $"{nameof(capacity)} must be >= 0", nameof(capacity));

            this.Capacity = capacity;
            this.StartIndexA = startIndexA;
            this.StartIndexB = startIndexB;

            this.isA = true;
            this.Index = startIndexA;
            this.Started = false;
            this.Ended = false;

            // Divide the probe limit in half. We always start with A, so
            // if the probe limit is an odd number, A's limit will be odd
            // and 1 bigger than B's.
            int probeLimitB = probeLimit >> 1;
            int probeLimitA = probeLimit - probeLimitB;

            // When indexA is near to but smaller than indexB, the A cursor
            // could end up straying into cursor B's range. So limit the
            // probe depth of A. Same applies in reverse if indexB < indexA.
            // If they are precisely the same then prefer cursorA. cursorB
            // will not be used at all.
            if(startIndexA <= startIndexB)
            {
                int maxProbeA = startIndexB - startIndexA;

                if(probeLimitA > maxProbeA)
                {
                    // Calculate how many probes A can't do
                    int probeExcessA = probeLimitA - maxProbeA;

                    // Give these probes to B.
                    probeLimitA -= probeExcessA;
                    probeLimitB += probeExcessA;
                }
            }
            else
            {
                int maxProbeB = startIndexA - startIndexB;

                if(probeLimitB > maxProbeB)
                {
                    // Calculate how many probes B can't do
                    int probeExcessB = probeLimitB - maxProbeB;

                    // Give these probes to A.
                    probeLimitB -= probeExcessB;
                    probeLimitA += probeExcessB;
                }
            }

            this.cursorA = new Cursor(startIndexA, capacity, probeLimitA);
            this.cursorB = new Cursor(startIndexB, capacity, probeLimitB);
        }

        public bool MoveNext()
        {
            if(!Started)
                Started = true;

            if(isA)
            {
                bool result = cursorA.MoveNext();
                if(result)
                {
                    Index = cursorA.Index;
                    isA = false;    // Prefer cursor B next time

                    return true;
                }
                else
                {
                    if(cursorB.MoveNext())
                    {
                        Index = cursorB.Index;
                        return true;
                    }

                    this.Ended = true;
                    return false;
                }
            }
            else
            {
                bool result = cursorB.MoveNext();
                if(result)
                {
                    Index = cursorB.Index;
                    isA = true;     // Prefer cursor A next time

                    return true;
                }
                else
                {
                    if(cursorA.MoveNext())
                    {
                        Index = cursorA.Index;
                        return true;
                    }

                    this.Ended = true;
                    return false;
                }

            }
        }

        public int Index { get; private set; }
        public bool Started { get; private set; }
        public bool Ended { get; private set; }
        public int Capacity { get; }
        public int StartIndexA { get; }
        public int StartIndexB { get; }
        public int ProbeLimitA => cursorA.MoveLimit;
        public int ProbeLimitB => cursorB.MoveLimit;

        public int MoveCount => cursorA.MoveCount + cursorB.MoveCount;

    }
}

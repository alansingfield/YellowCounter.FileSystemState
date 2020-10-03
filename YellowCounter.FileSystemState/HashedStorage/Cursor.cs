using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    public partial class HashBucket2<T>
    {
        /// <summary>
        /// This logic controls the cursor position for scanning through a wrap-around
        /// array.
        /// </summary>
        public struct Cursor
        {
            /// <summary>
            /// Calculates a moduloed index within a wrap-around array
            /// </summary>
            /// <param name="startIndex">Start index in array, from 0..capacity-1</param>
            /// <param name="capacity">Length of array</param>
            /// <param name="moveLimit">Limit the number of iterations to avoid infinite loop</param>
            public Cursor(int startIndex, int capacity, int moveLimit)
            {
                if(startIndex < 0 || startIndex >= capacity)
                    throw new ArgumentException(
                        $"{nameof(startIndex)} must be between 0 and ${nameof(capacity)}-1",
                        nameof(startIndex));

                if(moveLimit < 0)
                    throw new ArgumentException(
                        $"{nameof(moveLimit)} must be >= 0", nameof(moveLimit));

                if(capacity < 0)
                    throw new ArgumentException(
                        $"{nameof(capacity)} must be >= 0", nameof(capacity));

                this.Capacity = capacity;
                this.MoveLimit = moveLimit;

                this.Index = startIndex;
                this.StartIndex = startIndex;
                this.MoveCount = 0;
                this.Started = false;
                this.Ended = false;
            }

            /// <summary>
            /// Advance the cursor. The first call sets Started to true. Subsequent calls increase
            /// the Index and MoveCount fields. The Index field wraps around, the MoveCount does not.
            /// When we make the (MoveLimit+1)st move, it will return False and the Ended flag is
            /// set to true. Any further calls will always return false and leave the index where it is.
            /// </summary>
            /// <returns>True if we have not exceeded maximum number of iterations (MoveLimit)</returns>
            public bool MoveNext()
            {
                if(Ended)
                    return false;

                if(!Started)
                {
                    Started = true;
                }
                else
                {
                    MoveCount++;
                    Index++;
                }

                if(Index >= Capacity)
                    Index %= Capacity;

                var result = MoveCount < MoveLimit;

                if(!result)
                    Ended = true;

                return result;
            }

            /// <summary>
            /// Have we moved the cursor yet? True after first call to MoveNext()
            /// </summary>
            public bool Started { get; private set; }
            /// <summary>
            /// Have we reached the limit of the number of moves?
            /// </summary>
            public bool Ended { get; private set; }

            /// <summary>
            /// Array index, will be in range 0..Capacity-1
            /// </summary>
            public int Index { get; private set; }
            public int MoveCount { get; private set; }

            public int StartIndex { get; }
            public int Capacity { get; }
            public int MoveLimit { get; }
        }
    }
}

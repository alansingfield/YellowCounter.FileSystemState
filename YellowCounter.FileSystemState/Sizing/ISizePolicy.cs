using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Sizing
{
    public interface ISizePolicy
    {
        /// <summary>
        /// Calculate whether the fill factor is too high or too low
        /// and compute a new recommended size if required.
        /// </summary>
        /// <param name="usage">Intended usage count</param>
        /// <param name="capacity">Current capacity of buffer</param>
        /// <returns>Null for no change or the new size</returns>
        int? MustResize(int usage, int capacity);
    }
}

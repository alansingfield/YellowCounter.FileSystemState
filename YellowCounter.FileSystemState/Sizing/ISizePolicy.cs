using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Sizing
{
    public interface ISizePolicy
    {
        /// <summary>
        /// Upon creation, set this to the current size of the buffer.
        /// When MustResize() returns TRUE, read this to find what the new
        /// size should be.
        /// </summary>
        int Capacity { get; set; }
        /// <summary>
        /// Before adding items to the buffer, calculate what the new number
        /// of items will be. This will return TRUE if you
        /// must resize the buffer before adding the items in.
        /// </summary>
        /// <param name="usage">Intended usage count</param>
        /// <returns>Whether to resize or not</returns>
        bool MustResize(int usage);
    }
}

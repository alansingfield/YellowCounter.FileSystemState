using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashCodes
{
    /// <summary>
    /// Wrapper for System.HashCode so we can test with a deterministic hashcode
    /// algorithm. The standard implementation gets a different seed each time
    /// it is run.
    /// </summary>
    public struct StandardHashCode : IHashCode
    {
        private HashCode hashCode;
        public void Add(char value)
        {
            hashCode.Add(value);
        }

        public int ToHashCode() => hashCode.ToHashCode();
    }
}

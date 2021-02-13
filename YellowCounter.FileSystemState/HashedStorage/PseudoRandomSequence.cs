using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.HashedStorage
{
    /// <summary>
    /// Non-repeating pseudo-random sequence
    /// </summary>
    public static class PseudoRandomSequence
    {
        public static uint Permute(uint x)
        {
            return permuteQPR(permuteQPR(x) ^ 0x5bf03635);
        }

        public static int Permute(int x)
        {
            return unchecked((int)Permute((uint)x));
        }

        private static uint permuteQPR(uint x)
        {
            // from https://preshing.com/20121224/how-to-generate-a-sequence-of-unique-random-integers/
            // https://github.com/preshing/RandomSequence/blob/master/randomsequence.h

            const uint prime = 4294967291;
            const uint primeDiv2 = 2147483645;

            // The 5 integers out of range are mapped to themselves.
            if(x >= prime)
                return x;

            uint residue = (uint)(((ulong)x * x) % prime);
            return (x <= primeDiv2) ? residue : prime - residue;
        }
    }
}

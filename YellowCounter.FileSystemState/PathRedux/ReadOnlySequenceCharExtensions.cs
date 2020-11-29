using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.PathRedux
{
    public static class ReadOnlySequenceCharExtensions
    {
        /// <summary>
        /// Allocate a new string and populate it with the character sequence seq.
        /// This allocates the sring once and so is more efficient than piecing the
        /// string together with a StringBuilder.
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static string CreateString(this ReadOnlySequence<char> seq)
        {
            int totalLen = 0;

            // Calculate total length of string to create, as we have to specify
            // it upfront.
            foreach(var itm in seq)
            {
                totalLen += itm.Length;
            }

            return String.Create(
                length: totalLen,
                state: (object)null,
                (chars, state) =>
                {
                    var cursor = 0;

                    // Loop through each segment of memory.
                    foreach(var itm in seq)
                    {
                        // How long is our current bit of text?
                        int len = itm.Length;

                        // Copy the text from the buffer to the result string.
                        itm.Span.CopyTo(chars.Slice(cursor, len));

                        // Move the cursor along.
                        cursor += len;
                    }
                });
        }
    }
}

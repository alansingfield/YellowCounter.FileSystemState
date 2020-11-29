using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.PathRedux
{
    public class CharBuffer
    {

        private Memory<char> buffer;
        private int pos;

        public CharBuffer(int capacity)
        {
            buffer = new char[capacity];
        }

        public int Capacity => buffer.Length;

        public void Resize(int capacity)
        {
            if(capacity < pos)
                throw new Exception("Cannot resize because data truncation would occur");

            var newBuffer = new char[capacity];

            this.buffer.CopyTo(newBuffer);

            this.buffer = newBuffer;
        }

        /// <summary>
        /// Store the text provided at the next free position in our buffer. This will
        /// always be at the end of the buffer. Return the index position we stored at.
        /// The text must not contain a null terminator \0 as this would truncate the
        /// text on retrieval.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int Store(ReadOnlySpan<char> input)
        {
            // We need space for our text, our null terminator, and an extra
            // null terminator for the end of the buffer.
            if(input.Length + pos + 1 >= buffer.Length)
                return -1;

            var bufSpan = buffer.Span;

            // Return current buffer start position as the result.
            var result = pos;

            // Write the text into our buffer
            input.CopyTo(bufSpan.Slice(pos, input.Length));
            pos += input.Length;

            // Null terminate
            bufSpan[pos] = '\0';
            pos++;

            return result;
        }

        /// <summary>
        /// Returns TRUE if arg matches the text at position index in our buffer.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool Match(ReadOnlySpan<char> arg, int index)
        {
            var bufSpan = buffer.Span;

            if(bufSpan.Slice(index, arg.Length).SequenceEqual(arg))
            {
                // Check for null terminator so we don't match to a
                // longer string.
                if(bufSpan[index + arg.Length] == '\0')
                    return true;
            }

            // not found.
            return false;
        }

        public ReadOnlySpan<char> Retrieve(int pos)
        {
            var bufSpan = buffer.Span;

            var begin = bufSpan.Slice(pos);

            int len = begin.IndexOf('\0');

            return begin.Slice(0, len);
        }

        public Enumerator GetEnumerator()
        {
            var bufSpan = buffer.Span;

            return new Enumerator(bufSpan);
        }

        /// <summary>
        /// Enumerate through the entire contents of the buffer.
        /// </summary>
        public ref struct Enumerator
        {
            private int pos;
            private int len;
            ReadOnlySpan<char> bufSpan;
            Item current;

            public Enumerator(ReadOnlySpan<char> bufSpan)
            {
                pos = -1;
                len = 0;
                this.bufSpan = bufSpan;
                current = new Item();
            }

            public readonly Item Current => current;
            public bool MoveNext()
            {
                // Advance past zero terminator and previous string.
                pos += 1 + len;

                var tail = bufSpan.Slice(pos);

                // Reached the end? End enumerating. The end of the buffer
                // has a double null terminator \0\0.
                if(tail[0] == '\0')
                    return false;

                len = tail.IndexOf('\0');

                this.current.Span = tail.Slice(0, len);
                this.current.Pos = pos;

                return true;
            }
        }

        public ref struct Item
        {
            public ReadOnlySpan<char> Span;
            public int Pos;
        }


        /// <summary>
        /// Given a set of indices, retrieve the text at each position and concatenate
        /// into a single sequence of chars.
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public ReadOnlySequence<char> Retrieve(IEnumerable<int> indices)
        {
            Segment<char> root = null;
            Segment<char> current = null;

            int len = 0;

            foreach(var idx in indices)
            {
                var tail = buffer.Slice(idx);
                len = tail.Span.IndexOf('\0');
                var text = tail.Slice(0, len);

                if(root == null)
                {
                    root = new Segment<char>(text);
                    current = root;
                }
                else
                {
                    current = current.Add(text);
                }
            }

            return new ReadOnlySequence<char>(root, 0, current, len);
        }

        class Segment<T> : ReadOnlySequenceSegment<T>
        {
            public Segment(ReadOnlyMemory<T> memory)
                => Memory = memory;
            public Segment<T> Add(ReadOnlyMemory<T> mem)
            {
                var segment = new Segment<T>(mem);
                segment.RunningIndex = RunningIndex +
                            Memory.Length;
                Next = segment;
                return segment;
            }
        }

    }

}

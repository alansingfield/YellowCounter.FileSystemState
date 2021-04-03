using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Options;

namespace YellowCounter.FileSystemState.PathRedux
{
    /// <summary>
    /// Storing a long list of full paths from a recursive directory search involves
    /// a lot of repeats:
    /// C:\abc\def
    /// C:\abc\def\ghi
    /// C:\abc\def\jkl
    /// C:\abc\def\mno
    /// 
    /// This class implements a Parent Pointer Tree, it splits the path by the directory
    /// separator, stores the final text after the \, then a pointer to the entry for
    /// the parent directory. This occurs recursively so we only store the text for each
    /// folder name once.
    /// </summary>
    public class PathStorage : IPathStorage
    {
        private HashedCharBuffer buf;
        private HashBucket<int> buckets;
        private List<Entry> entries;
        private const int Root = -1;    // The root entry's ParentIdx is set to this.

        private PathStorageOptions options;

        private Func<IHashCode> newHashCode;

        public PathStorage(PathStorageOptions options = null)
        {
            this.options = (options == null)
                ? new PathStorageOptions()
                : options.Clone();
            
            // Need to use the same HashCode function as the HashedCharBuffer.
            this.newHashCode = options.HashedCharBufferOptions.NewHashCode;

            buf = new HashedCharBuffer(options.HashedCharBufferOptions);

            buckets = new HashBucket<int>(options.HashBucketOptions);

            entries = new List<Entry>();
        }

        public int Store(ReadOnlySpan<char> arg)
        {
            // Generate a hash of the text supplied in arg.
            var hash = newHashCode().HashSequence(arg);

            // This hash then will give us a number of candidate indexes to try
            foreach(var idx in buckets.Retrieve(hash))
            {
                // Does the arg supplied match the text we stored previously
                // at this index?
                if(match(idx, arg))
                {
                    // Yes - return the index of an existing path.
                    return idx;
                }
            }

            int parentIdx;
            int textRef;

            // Find a slash or backslash.
            int slashPos = arg.LastIndexOfAny(new[] { '\\', '/' });

            // If there is no slash or backslash it is the root entry "C:\" or
            // similar.
            if(slashPos == -1)
            {
                parentIdx = Root;
                textRef = buf.Store(arg);
            }
            else
            {
                // Recursively call back to ourselves to store all text
                // up to the parent directory name. This might find an
                // existing entry or need to create one.
                parentIdx = this.Store(arg.Slice(0, slashPos));

                // Store the text from the slash onwards as our entry.
                textRef = buf.Store(arg.Slice(slashPos));
            }

            int entryIdx = entries.Count;
            entries.Add(new Entry(textRef, parentIdx));

            storeHashInLookup(hash, entryIdx);

            return entryIdx;
        }

        private void storeHashInLookup(int hash, int entryIdx)
        {
            resizeIfNeeded(1);

            if(!buckets.TryStore(hash, entryIdx))
                throw new Exception("Unable to store in lookup");
        }

        private void resizeIfNeeded(int headroom)
        {
            int? newCapacity = options.SizePolicy.MustResize(
                buckets.Usage + headroom,
                buckets.Capacity);

            if(newCapacity != null)
                rebuildBuckets(newCapacity.Value);
        }

        private void rebuildBuckets(int newCapacity)
        {
            var hbOptions = this.options.HashBucketOptions.Clone();
            hbOptions.Capacity = newCapacity;

            var replacement = new HashBucket<int>(hbOptions);

            // Re-hash all our existing entries and try storing into the replacement 
            // hashbucket.
            for(int idx = 0; idx < entries.Count; idx++)
            {
                int hash = rehashEntry(idx);

                if(!replacement.TryStore(hash, idx))
                    throw new Exception("Error in rebuilding HashBucket, capacity exceeded");
            }

            // If we get to here, we've successfully rebuilt the buckets.
            this.buckets.Dispose();
            this.buckets = replacement;
        }

        private int rehashEntry(int idx)
        {
            var hashCode = newHashCode();

            // Take our index point, calculate all ancestors back to root
            var sequence = buf.Retrieve(indicesFromRootTo(idx));

            foreach(var mem in sequence)
            {
                foreach(var ch in mem.Span)
                {
                    hashCode.Add(ch);
                }
            }

            int hash = hashCode.ToHashCode();
            return hash;
        }

        /// <summary>
        /// Copy the stored path for the given index to a newly allocated string.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public string CreateString(int idx)
        {
            return Retrieve(idx).CreateString();
        }

        /// <summary>
        /// Return the stored path for the given index as a sequence of ReadOnlyMemory&lt;char&gt;
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public ReadOnlySequence<char> Retrieve(int idx)
        {
            return buf.Retrieve(indicesFromRootTo(idx));
        }

        /// <summary>
        /// Indices which start at the root and end up at idx
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private IEnumerable<int> indicesFromRootTo(int idx) => ancestorsAndSelf(idx).Reverse();

        /// <summary>
        /// Follow the parent, grandparent chain back to the root.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private IEnumerable<int> ancestorsAndSelf(int idx)
        {
            int cursorIdx = idx;

            while(cursorIdx != Root)
            {
                var entry = entries[cursorIdx];

                yield return entry.TextRef;
                cursorIdx = entry.ParentIdx;
            }
        }

        private bool match(int idx, ReadOnlySpan<char> arg)
        {
            int argStart = arg.Length;
            int cursorIdx = idx;

            while(true)
            {
                var entry = entries[cursorIdx];

                var text = buf.Retrieve(entry.TextRef);

                argStart -= text.Length;

                if(argStart < 0)
                    return false;

                var argSlice = arg.Slice(argStart, text.Length);

                if(!text.SequenceEqual(argSlice))
                    return false;

                // Loop round to our parent entry
                cursorIdx = entry.ParentIdx;

                if(cursorIdx == Root)
                {
                    // If the target has no parent, and we've examined all of arg
                    // then we've got a correct match
                    if(argStart == 0)
                        return true;

                    return false;
                }
            }

        }

        private readonly struct Entry
        {
            public Entry(int textRef, int parentIdx)
            {
                this.TextRef = textRef;
                this.ParentIdx = parentIdx;
            }

            public int TextRef { get; }
            public int ParentIdx { get; }
        }

    }
}

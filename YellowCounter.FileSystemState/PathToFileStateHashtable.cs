using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.IO.Enumeration;
using YellowCounter.FileSystemState.PathRedux;
using System.Diagnostics;
using System.IO;

namespace YellowCounter.FileSystemState
{
    internal class PathToFileStateHashtable
    {
        Dictionary<int, List<FileState>> dict;
        private readonly IPathStorage pathStorage;

        HashBucket<FileState> hb;

        public PathToFileStateHashtable(IPathStorage pathStorage) 
        {
            dict = new Dictionary<int, List<FileState>>();

            hb = new HashBucket<FileState>(new HashBucketOptions()
            {
                Capacity = 100,
                LinearSearchLimit = 10
            });

            this.pathStorage = pathStorage;
        }

        internal void Mark(ref FileSystemEntry input)
        {
            int dirRef = pathStorage.Store(input.Directory);
            int filenameRef = pathStorage.Store(input.FileName);

            int hashCode = HashCode.Combine(dirRef.GetHashCode(), filenameRef.GetHashCode());

            //var fileStates = hb.Retrieve(hashCode);

            // Normally there will only be 1 but we could get a hash collision.
            foreach(ref var existing in hb.RetrieveX(hashCode))
            {
                // We've only matched on hashcode so far, so there could be false
                // matches in here. Do a proper comparision on filename/directory.
                if(existing.FilenameRef == filenameRef && existing.DirectoryRef == dirRef)
                {
                    // Found the file; compare to our existing record so we can
                    // detect if it has been modified.
                    markExisting(ref existing, input);

                    return;
                }
            }

            FileState fileState;

            fileState.Flags = FileStateFlags.Created | FileStateFlags.Seen;

            fileState.DirectoryRef = dirRef;
            fileState.FilenameRef = filenameRef;

            fileState.LastWriteTimeUtc = input.LastWriteTimeUtc;
            fileState.Length = input.Length;

            if(!hb.TryStore(hashCode, fileState))
            {
                rebuildLookup(fileState, headroom: 1);
            }
        }

        private void rebuildLookup(FileState? newFileState, int headroom)
        {
            foreach(var options in this.hb.SizeOptions(headroom))
            {
                var replacement = new HashBucket<FileState>(options);

                foreach(var fileState in this.hb)
                {
                    // Don't try to store the doomed items
                    if(!fileState.Flags.HasFlag(FileStateFlags.Doomed))
                    {
                        if(!replacement.TryStore(fileState))
                            continue;
                    }
                }

                if(newFileState != null)
                {
                    if(!replacement.TryStore(newFileState.Value))
                        continue;
                }

                // If we get to here, we've successfully re-hashed all contents
                this.hb = replacement;

                return;
            }

            throw new Exception("Unable to rebuild FileState hashtable");
        }

        private void markExisting(ref FileState fs, FileSystemEntry input)
        {
            // Mark that we've seen the file.
            fs.Flags |= FileStateFlags.Seen;

            // Has it changed since we last saw it?
            if(fs.LastWriteTimeUtc != input.LastWriteTimeUtc
                || fs.Length != input.Length)
            {
                fs.Flags |= FileStateFlags.Changed;

                // Update the last write time / file length.
                fs.LastWriteTimeUtc = input.LastWriteTimeUtc;
                fs.Length = input.Length;
            }
        }


        public ReadOnlySpan<FileState> AsSpan() => hb.AsSpan();

        //public IEnumerable<FileState> Read()
        //{
        //    for(int pos = 0; pos < this.hb.Capacity, pos++)
        //    {

        //    }

        //    var l = new List<FileState>();

        //    foreach(var x in this.hb)
        //    {
        //        l.Add(x);
        //        //yield return x;
        //    }

        //    return l;
        //}

        public void Sweep()
        {
            int doomedCount = 0;

            /// ??? Not sure why I can do this, Readonlyspan ????
            foreach(ref var fileState in this.hb)
            {
                // All elements that have not been seen on the last sweep are now
                // no longer any use to us. Because we assume a contiguous block of
                // items when retrieving, we can't remove the item from the hashbucket
                // at the moment. (that is done at rebuild time)
                // Instead we mark it as a placekeeper to be skipped over.
                if(fileState.Flags != FileStateFlags.None 
                    && !fileState.Flags.HasFlag(FileStateFlags.Seen))
                {
                    // why can I change fileState if its a ref???
                    fileState.Flags = FileStateFlags.Doomed;

                    doomedCount++;
                }
            }

            // If using too much space now, rebuild the lookup to be smaller.

            if(this.hb.Occupancy - doomedCount < this.hb.Capacity * 0.5)
            {
                rebuildLookup(null, -doomedCount);
            }

            //foreach(var fileState in this.hb)
            //{
            //    if(!fileState.Flags.HasFlag(FileStateFlags.Seen))
            //        hb.Remove(
            //}

            //var toRemove = new List<int>();

            //// Go through every list of filestates in our state dictionary
            //foreach(var (hash, list) in dict)
            //{
            //    // Remove any item in the list which we didn't see on the last mark
            //    // phase (every item that is seen gets the LastSeenVersion updated)
            //    //list.RemoveAll(x => x.LastSeenVersion != version);

            //    list.RemoveAll(x => !x.Flags.HasFlag(FileStateFlags.Seen));

            //    // In the normal case where there are no hash collisions, this will
            //    // remove the one and only item from the list. We can then remove
            //    // the hash entry from the dictionary.
            //    // If there was a hash collision, the reduced-size list would remain.
            //    if(list.Count == 0)
            //    {
            //        toRemove.Add(hash);
            //    }

            //    // Clear the flags on all remaining items.
            //    foreach(var x in list)
            //    {
            //        x.Flags = FileStateFlags.None;
            //    }
            //}

            //// We can't remove the items while iterating so remove here instead.
            //foreach(var hash in toRemove)
            //{
            //    dict.Remove(hash);
            //}
        }
    }

}

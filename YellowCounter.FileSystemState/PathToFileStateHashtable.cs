using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.IO.Enumeration;
using YellowCounter.FileSystemState.PathRedux;
using System.Diagnostics;
using System.IO;
using YellowCounter.FileSystemState.HashedStorage;

namespace YellowCounter.FileSystemState
{
    internal class PathToFileStateHashtable
    {
        FileStateReferenceSet dict;
        private readonly IPathStorage pathStorage;

        public PathToFileStateHashtable(IPathStorage pathStorage) 
        {
            dict = new FileStateReferenceSet(new FileStateReferenceSetOptions()
            {
                Capacity = 100,
                ChunkSize = 32,
            });

            this.pathStorage = pathStorage;
        }

        internal void Mark(in FileSystemEntry input)
        {
            // Look up the directory string and filename string, convert to a reference
            // number in pathStorage.
            int dirRef = pathStorage.Store(input.Directory);
            int filenameRef = pathStorage.Store(input.FileName);

            // Find the directory/filename in the dictionary
            int index = dict.IndexOf((dirRef, filenameRef));
            if(index != -1)
            {
                // In-place update of the found element
                ref var fileState = ref dict.ElementAt(index);

                markExisting(ref fileState, in input);
            }
            else
            {
                // Not found? Create a new one and add it in
                var fileState = newFileState(in input, dirRef, filenameRef);

                dict.Add((dirRef, filenameRef), fileState);
            }
        }

        private static FileState newFileState(in FileSystemEntry input, int dirRef, int filenameRef)
        {
            FileState fileState;

            // It's a new one so mark as "Created". Also set the Seen flag.
            fileState.Flags = FileStateFlags.Created | FileStateFlags.Seen;

            fileState.DirectoryRef =        dirRef;
            fileState.FilenameRef =         filenameRef;

            fileState.LastWriteTimeUtc =    input.LastWriteTimeUtc;
            fileState.Length =              input.Length;

            return fileState;
        }

        private void markExisting(ref FileState fs, in FileSystemEntry input)
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

        public HashBucket2<FileState>.Enumerator GetEnumerator() => dict.GetEnumerator();


        public void Sweep()
        {
            foreach(ref var fileState in this.dict)
            {
                // All elements that have not been seen on the last sweep are now
                // no longer any use to us and can be deleted.
                if(!fileState.Flags.HasFlag(FileStateFlags.Seen))
                {
                    dict.Delete(ref fileState);
                }
                else
                {
                    // Clear the flags for next time.
                    fileState.Flags = FileStateFlags.None;
                }
            }
        }
    }

}

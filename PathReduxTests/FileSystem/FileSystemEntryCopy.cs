using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Text;

namespace PathReduxTests.FileSystem
{
    /// <summary>
    /// We can't store ref structs in an array (since the references would go out-of-scope)
    /// so we need to copy all the values to a less restrictive (but more memory-hungry) type
    /// </summary>
    public class FileSystemEntryCopy
    {
        public FileAttributes Attributes { get; set; }
        public DateTimeOffset CreationTimeUtc { get; set; }
        public String Directory { get; set; }
        public String FileName { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsHidden { get; set; }
        public DateTimeOffset LastAccessTimeUtc { get; set; }
        public DateTimeOffset LastWriteTimeUtc { get; set; }
        public long Length { get; set; }

        public static FileSystemEntryCopy Copy(in FileSystemEntry entry)
        {
            return new FileSystemEntryCopy()
            {
                Attributes = entry.Attributes,
                CreationTimeUtc = entry.CreationTimeUtc,
                Directory = entry.Directory.ToString(),
                FileName = entry.FileName.ToString(),
                IsDirectory = entry.IsDirectory,
                IsHidden = entry.IsHidden,
                LastAccessTimeUtc = entry.LastAccessTimeUtc,
                LastWriteTimeUtc = entry.LastWriteTimeUtc,
                Length = entry.Length,
            };
        }

        public FileSystemEntry ToFse()
        {
            var cheater = new Cheater();

            cheater.Overlay = new FileSystemEntryOverlay();

            cheater.Overlay.Attributes = this.Attributes;
            
            cheater.Overlay.Directory = this.Directory;
            cheater.Overlay.FileName = this.FileName;

            return cheater.FSE;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public ref struct Cheater
    {
        [FieldOffset(0)]
        public FileSystemEntry FSE;

        [FieldOffset(0)]
        public FileSystemEntryOverlay Overlay;


    }

    public ref struct FileSystemEntryOverlay
    {
        public FileAttributes Attributes;
        public DateTimeOffset CreationTimeUtc;
        public ReadOnlySpan<char> Directory;
        public ReadOnlySpan<char> FileName;
        public bool IsDirectory;
        public bool IsHidden;
        public DateTimeOffset LastAccessTimeUtc;
        public DateTimeOffset LastWriteTimeUtc;
        public long Length;
    }
}
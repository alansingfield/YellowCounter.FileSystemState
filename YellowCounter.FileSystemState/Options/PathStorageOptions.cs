using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Sizing;

namespace YellowCounter.FileSystemState.Options
{
    public class PathStorageOptions
    {
        public HashedCharBufferOptions HashedCharBufferOptions { get; set; } = new HashedCharBufferOptions();
        public HashBucketOptions HashBucketOptions { get; set; } = new HashBucketOptions();
        public ISizePolicy SizePolicy { get; set; } = new SizePolicy();

        public PathStorageOptions Clone() => (PathStorageOptions)this.MemberwiseClone();
    }
}

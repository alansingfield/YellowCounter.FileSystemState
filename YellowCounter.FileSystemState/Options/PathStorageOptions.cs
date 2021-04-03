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
        public virtual HashedCharBufferOptions HashedCharBufferOptions { get; set; } = new HashedCharBufferOptions();
        public virtual HashBucketOptions HashBucketOptions { get; set; } = new HashBucketOptions();
        public virtual ISizePolicy SizePolicy { get; set; } = new SizePolicy();
        public virtual PathStorageOptions Clone() => (PathStorageOptions)this.MemberwiseClone();
    }
}

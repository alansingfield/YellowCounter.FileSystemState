using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Sizing;

namespace YellowCounter.FileSystemState.Options
{
    public class HashedCharBufferOptions
    {
        public Func<IHashCode> NewHashCode { get; set; } = () => new StandardHashCode();
        public int InitialCharCapacity { get; set; } = 1024;
        public HashBucketOptions HashBucketOptions { get; set; } = new HashBucketOptions();
        public ISizePolicy SizePolicy { get; set; } = new SizePolicy();

        public HashedCharBufferOptions Clone() => (HashedCharBufferOptions)this.MemberwiseClone();
    }
}

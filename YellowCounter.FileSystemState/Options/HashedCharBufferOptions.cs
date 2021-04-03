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
        public ISizePolicy HashSizePolicy { get; set; } = new SizePolicy();
        public ISizePolicy CharSizePolicy { get; set; } = new SizePolicy(new SizePolicyOptions()
        {
            FillFactor = 100,       // Allow dense packing
            GrowthFactor = 100,     // Double in size each time
            MinCapacity = 0,        // Zero elements OK
            MinFillFactor = 0,      // Never shrink
        });

        public HashedCharBufferOptions Clone() => (HashedCharBufferOptions)this.MemberwiseClone();
    }
}

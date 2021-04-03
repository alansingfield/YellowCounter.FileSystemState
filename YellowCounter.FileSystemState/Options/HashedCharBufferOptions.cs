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
        public virtual Func<IHashCode> NewHashCode { get; set; } = () => new StandardHashCode();
        public virtual int InitialCharCapacity { get; set; } = 1024;
        public virtual HashBucketOptions HashBucketOptions { get; set; } = new HashBucketOptions();
        public virtual ISizePolicy HashSizePolicy { get; set; } = new SizePolicy();
        public virtual ISizePolicy CharSizePolicy { get; set; } = new SizePolicy(new SizePolicyOptions()
        {
            FillFactor = 100,       // Allow dense packing
            GrowthFactor = 100,     // Double in size each time
            MinCapacity = 0,        // Zero elements OK
            MinFillFactor = 0,      // Never shrink
        });

        public virtual HashedCharBufferOptions Clone() => (HashedCharBufferOptions)this.MemberwiseClone();
    }
}

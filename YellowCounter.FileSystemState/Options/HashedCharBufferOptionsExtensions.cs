using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashCodes;
using YellowCounter.FileSystemState.Sizing;

namespace YellowCounter.FileSystemState.Options
{
    public static class HashedCharBufferOptionsExtensions
    {
        public static HashedCharBufferOptions WithNewHashCode(this HashedCharBufferOptions hashedCharBufferOptions,
            Func<IHashCode> newHashCode)
        {
            hashedCharBufferOptions.NewHashCode = newHashCode;
            return hashedCharBufferOptions;
        }

        public static HashedCharBufferOptions WithInitialCharCapacity(this HashedCharBufferOptions hashedCharBufferOptions,
            int initialCharCapacity)
        {
            hashedCharBufferOptions.InitialCharCapacity = initialCharCapacity;
            return hashedCharBufferOptions;
        }

        public static HashedCharBufferOptions WithNHashBucketOptions(this HashedCharBufferOptions hashedCharBufferOptions,
            HashBucketOptions hashBucketOptions)
        {
            hashedCharBufferOptions.HashBucketOptions = hashBucketOptions;
            return hashedCharBufferOptions;
        }

        public static HashedCharBufferOptions WithHashSizePolicy(this HashedCharBufferOptions hashedCharBufferOptions,
            ISizePolicy hashSizePolicy)
        {
            hashedCharBufferOptions.HashSizePolicy = hashSizePolicy;

            return hashedCharBufferOptions;
        }

        public static HashedCharBufferOptions WithCharSizePolicy(this HashedCharBufferOptions hashedCharBufferOptions,
            ISizePolicy charSizePolicy)
        {
            hashedCharBufferOptions.CharSizePolicy = charSizePolicy;

            return hashedCharBufferOptions;
        }
    }
}

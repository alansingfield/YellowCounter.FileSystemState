using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Sizing;

namespace YellowCounter.FileSystemState.Options
{
    public static class ReferenceSetOptionsExtensions
    {
        public static ReferenceSetOptions WithHashBucketOptions(this ReferenceSetOptions referenceSetOptions,
            HashBucketOptions hashBucketOptions)
        {
            referenceSetOptions.HashBucketOptions = hashBucketOptions;

            return referenceSetOptions;
        }

        public static ReferenceSetOptions WithSizePolicy(this ReferenceSetOptions referenceSetOptions,
            ISizePolicy sizePolicy)
        {
            referenceSetOptions.SizePolicy = sizePolicy;

            return referenceSetOptions;
        }
    }
}

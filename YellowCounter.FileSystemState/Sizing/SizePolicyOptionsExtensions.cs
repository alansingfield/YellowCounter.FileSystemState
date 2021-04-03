using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Sizing
{
    public static class SizePolicyOptionsExtensions
    {
        public static SizePolicyOptions WithFillFactor(this SizePolicyOptions sizePolicyOptions,
            int fillFactor)
        {
            sizePolicyOptions.FillFactor = fillFactor;
            return sizePolicyOptions;
        }

        public static SizePolicyOptions WithMinCapacity(this SizePolicyOptions sizePolicyOptions,
            int minCapacity)
        {
            sizePolicyOptions.MinCapacity = minCapacity;
            return sizePolicyOptions;
        }

        public static SizePolicyOptions WithMinFillFactor(this SizePolicyOptions sizePolicyOptions,
            int minFillFactor)
        {
            sizePolicyOptions.MinFillFactor = minFillFactor;
            return sizePolicyOptions;
        }

        public static SizePolicyOptions WithGrowthFactor(this SizePolicyOptions sizePolicyOptions,
            int growthFactor)
        {
            sizePolicyOptions.GrowthFactor = growthFactor;
            return sizePolicyOptions;
        }

        public static SizePolicyOptions WithShrinkToFillFactor(this SizePolicyOptions sizePolicyOptions,
            int shrinkTofillFactor)
        {
            sizePolicyOptions.ShrinkToFillFactor = shrinkTofillFactor;
            return sizePolicyOptions;
        }
    }
}

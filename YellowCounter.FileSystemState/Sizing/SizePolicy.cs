using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Sizing
{
    public class SizePolicy : ISizePolicy
    {
        private SizePolicyOptions options;

        public SizePolicy(SizePolicyOptions options = null)
        {
            this.options = (options == null)
                ? new SizePolicyOptions()
                : options.Clone();

            VerifyOptions(this.options);
        }

        protected virtual void VerifyOptions(SizePolicyOptions options)
        {
            if(options.FillFactor < 1 || options.FillFactor > 100)
                throw new ArgumentOutOfRangeException(nameof(options.FillFactor),
                    $"Argument must be in the range 1..100");

            if(options.MinCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(options.MinCapacity),
                    $"Argument must be positive");

            if(options.MinFillFactor < 0 || options.MinFillFactor > 100)
                throw new ArgumentOutOfRangeException(nameof(options.MinFillFactor),
                    $"Argument must be in the range 0..100");

            if(options.GrowthFactor < 1 || options.GrowthFactor > 200)
                throw new ArgumentOutOfRangeException(nameof(options.GrowthFactor),
                    $"Argument must be in the range 1..200");

            if(options.ShrinkToFillFactor < 1 || options.ShrinkToFillFactor > 100)
                throw new ArgumentOutOfRangeException(nameof(options.ShrinkToFillFactor),
                    $"Argument must be in the range 1..100");

            if(options.MinFillFactor > options.FillFactor)
                throw new ArgumentOutOfRangeException(nameof(options.MinFillFactor),
                    $"Argument must not be greater than {nameof(options.FillFactor)}");

            if(options.ShrinkToFillFactor > options.FillFactor)
                throw new ArgumentOutOfRangeException(nameof(options.ShrinkToFillFactor),
                    $"Argument must not be greater than {nameof(options.FillFactor)}");
        }

        public virtual int? MustResize(int usage, int capacity)
        {
            var maxUsage = (int)(capacity * options.FillFactor / 100.0d);
            var minUsage = (int)(capacity * options.MinFillFactor / 100.0d);

            int newCapacity;

            // Make it bigger?
            if(usage > maxUsage)
            {
                newCapacity = (int)(
                    capacity 
                    * (100 + options.GrowthFactor) / 100.0d
                );
            }

            // Make it smaller?
            else if(usage < minUsage && usage >= options.MinCapacity)
            {
                // ShrinkToFillFactor is the target FillFactor after the shrink occurs.
                // Note that the MinCapactity may end up meaning that the target
                // FillFactor is lower.
                newCapacity = (int)(
                    usage 
                    * (100.0d / options.ShrinkToFillFactor)
                );
            }
            else
            {
                // No need to resize.
                return null;
            }

            // Make sure new size will fit the new usage (perhaps Capacity was zero
            // or we are expanding by a lot)
            if(newCapacity < usage)
            {
                // Calculate the new size 
                newCapacity = (int)(
                    usage                               // Start with the usage
                    * (100.0d / options.FillFactor)     // Compute size for the fill factor limit
                    * ((100 + options.GrowthFactor) / 100.0d)   // then grow again.
                    );
            }

            // Never go smaller than the minimum size.
            if(newCapacity < options.MinCapacity)
                newCapacity = options.MinCapacity;

            return newCapacity;
        }
    }

}

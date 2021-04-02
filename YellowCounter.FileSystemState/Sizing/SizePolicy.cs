﻿using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Sizing
{
    public class SizePolicy : ISizePolicy
    {
        private int _capacity;
        private SizePolicyOptions options;

        private int minUsage;
        private int maxUsage;

        public SizePolicy(SizePolicyOptions options)
        {
            verifyOptions(options);

            this.options = options.Clone();
            
            //this._capacity = options.InitialCapacity;
            //refresh();
        }

        private void verifyOptions(SizePolicyOptions options)
        {
            if(options.FillFactor < 1 || options.FillFactor > 100)
                throw new ArgumentOutOfRangeException(nameof(options.FillFactor),
                    $"Argument must be in the range 1..100");

            //if(options.InitialCapacity < 0)
            //    throw new ArgumentOutOfRangeException(nameof(options.InitialCapacity),
            //        $")} must be positive");

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

            //if(options.InitialCapacity < options.MinCapacity)
            //    throw new ArgumentOutOfRangeException(nameof(options.InitialCapacity),
            //        $")} must not be less than {nameof(options.MinCapacity)}");

            if(options.MinFillFactor > options.FillFactor)
                throw new ArgumentOutOfRangeException(nameof(options.MinFillFactor),
                    $"Argument must not be greater than {nameof(options.FillFactor)}");

            if(options.ShrinkToFillFactor > options.FillFactor)
                throw new ArgumentOutOfRangeException(nameof(options.ShrinkToFillFactor),
                    $"Argument must not be greater than {nameof(options.FillFactor)}");
        }

        public int Capacity 
        {
            get => this._capacity;
            set
            {
                this._capacity = value;
                refresh();
            }
        }

        private void refresh()
        {
            this.maxUsage = (int)(_capacity * options.FillFactor / 100.0d);
            this.minUsage = (int)(_capacity * options.MinFillFactor / 100.0d);
        }

        public bool MustResize(int usage)
        {
            int newCapacity;

            // Make it bigger?
            if(usage > this.maxUsage)
            {
                newCapacity = (int)(
                    this._capacity 
                    * (100 + options.GrowthFactor) / 100.0d
                );
            }

            // Make it smaller?
            else if(usage < this.minUsage && usage >= options.MinCapacity)
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
                return false;
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

            this._capacity = newCapacity;
            refresh();

            return true;
        }
    }

}

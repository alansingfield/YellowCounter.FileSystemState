using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Sizing
{
    public class SizePolicyOptions
    {
        /// <summary>
        /// Percentage maximum fill factor.
        /// </summary>
        public int FillFactor { get; set; } = 70;
        /// <summary>
        /// The Capacity returned will never be less than this.
        /// </summary>
        public int MinCapacity { get; set; } = 1024;
        /// <summary>
        /// If the fill factor is below this value, a shrink will occur.
        /// </summary>
        public int MinFillFactor { get; set; } = 30;
        /// <summary>
        /// Percentage to grow by when increasing size. E.g. 50 will expand a 100 item
        /// buffer to 150.
        /// </summary>
        public int GrowthFactor { get; set; } = 50;
        /// <summary>
        /// What the fill factor will be after a shrink occurs.
        /// </summary>
        public int ShrinkToFillFactor { get; set; } = 60;

        public SizePolicyOptions Clone() => (SizePolicyOptions)this.MemberwiseClone();
    }
}

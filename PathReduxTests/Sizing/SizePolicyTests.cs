using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Sizing;

namespace PathReduxTests.Sizing
{
    [TestClass]
    public class SizePolicyTests
    {
        [TestMethod]
        public void SizePolicyFillFactorExceededResizes()
        {
            var sizePolicy = new SizePolicy(new SizePolicyOptions()
            {
                FillFactor = 70,
                GrowthFactor = 100,
                MinCapacity = 10,
                MinFillFactor= 30,
                ShrinkToFillFactor= 60
            });

            // 700 items should fit
            sizePolicy.MustResize(700, 1000).ShouldBe(null);

            // 701 items will not, it will double the capacity since growth
            // factor is 100%
            sizePolicy.MustResize(701, 1000).ShouldBe(2000);
        }

        [TestMethod]
        public void SizePolicyFillFactorShrinkResizes()
        {
            var sizePolicy = new SizePolicy(new SizePolicyOptions()
            {
                FillFactor = 70,
                GrowthFactor = 100,
                MinCapacity = 10,
                MinFillFactor = 30,
                ShrinkToFillFactor = 60
            });

            // 300 items should fit
            sizePolicy.MustResize(300, 1000).ShouldBe(null);

            // 299 items is too small, it will resize so fill factor
            // is around 60%
            sizePolicy.MustResize(299, 1000).ShouldBe(498);
        }
    }
}

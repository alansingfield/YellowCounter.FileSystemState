using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using UnitTestCoder.Shouldly.Gen;
using YellowCounter.FileSystemState.Sizing;

namespace PathReduxTests.Sizing
{
    [TestClass]
    public class SizePolicyOptionsTests
    {
        [TestMethod]
        public void SizePolicyOptionsDefaults()
        {
            var options = new SizePolicyOptions();
            //ShouldlyTest.Gen(options, nameof(options));

            {
                options.ShouldNotBeNull();
                options.FillFactor.ShouldBe(70);
                options.MinCapacity.ShouldBe(1024);
                options.MinFillFactor.ShouldBe(0);
                options.GrowthFactor.ShouldBe(50);
                options.ShrinkToFillFactor.ShouldBe(60);
            }
        }

        [TestMethod]
        public void SizePolicyVerifyFillFactor0()
        {
            var options = new SizePolicyOptions()
            {
                FillFactor = 0
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be in the range 1..100 (Parameter 'FillFactor')");
        }

        [TestMethod]
        public void SizePolicyVerifyFillFactor1()
        {
            var options = new SizePolicyOptions()
            {
                MinFillFactor = 1,
                FillFactor = 1,
                ShrinkToFillFactor = 1,
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyFillFactor100()
        {
            var options = new SizePolicyOptions()
            {
                FillFactor = 100
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyFillFactor101()
        {
            var options = new SizePolicyOptions()
            {
                FillFactor = 101
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be in the range 1..100 (Parameter 'FillFactor')");
        }



        [TestMethod]
        public void SizePolicyVerifyMinCapacity0()
        {
            var options = new SizePolicyOptions()
            {
                MinCapacity = 0
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyMinCapacityMinus1()
        {
            var options = new SizePolicyOptions()
            {
                MinCapacity = -1
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be positive (Parameter 'MinCapacity')");
        }







        [TestMethod]
        public void SizePolicyVerifyMinFillFactorMinus1()
        {
            var options = new SizePolicyOptions()
            {
                MinFillFactor = -1
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be in the range 0..100 (Parameter 'MinFillFactor')");
        }

        [TestMethod]
        public void SizePolicyVerifyMinFillFactor1()
        {
            var options = new SizePolicyOptions()
            {
                MinFillFactor = 0
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyMinFillFactor100()
        {
            var options = new SizePolicyOptions()
            {
                FillFactor = 100,
                MinFillFactor = 100
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyMinFillFactor101()
        {
            var options = new SizePolicyOptions()
            {
                MinFillFactor = 101
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be in the range 0..100 (Parameter 'MinFillFactor')");
        }








        [TestMethod]
        public void SizePolicyVerifyGrowthFactor0()
        {
            var options = new SizePolicyOptions()
            {
                GrowthFactor = 0
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be in the range 1..200 (Parameter 'GrowthFactor')");
        }

        [TestMethod]
        public void SizePolicyVerifyGrowthFactor1()
        {
            var options = new SizePolicyOptions()
            {
                GrowthFactor = 1
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyGrowthFactor200()
        {
            var options = new SizePolicyOptions()
            {
                GrowthFactor = 200
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyGrowthFactor201()
        {
            var options = new SizePolicyOptions()
            {
                GrowthFactor = 201
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be in the range 1..200 (Parameter 'GrowthFactor')");
        }






        [TestMethod]
        public void SizePolicyVerifyShrinkFactor0()
        {
            var options = new SizePolicyOptions()
            {
                ShrinkToFillFactor = 0
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be in the range 1..100 (Parameter 'ShrinkToFillFactor')");
        }

        [TestMethod]
        public void SizePolicyVerifyShrinkFactor1()
        {
            var options = new SizePolicyOptions()
            {
                ShrinkToFillFactor = 1
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyShrinkFactor100()
        {
            var options = new SizePolicyOptions()
            {
                FillFactor = 100,
                ShrinkToFillFactor = 100
            };

            new SizePolicy(options);
        }

        [TestMethod]
        public void SizePolicyVerifyShrinkFactor201()
        {
            var options = new SizePolicyOptions()
            {
                FillFactor = 100,
                ShrinkToFillFactor = 101,
            };

            Should.Throw(() => {
                new SizePolicy(options);
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Argument must be in the range 1..100 (Parameter 'ShrinkToFillFactor')");
        }





    }
}

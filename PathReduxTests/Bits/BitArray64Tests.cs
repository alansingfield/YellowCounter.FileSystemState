using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Bits;

namespace PathReduxTests.Bits
{
    [TestClass]
    public class BitArray64Tests
    {
        [TestMethod]
        public void BitArrayPoolSimple()
        {
            var barr = new BitArray64(64);

            barr[0] = true;

            barr[0].ShouldBe(true);

            for(int i = 1; i < 64; i++)
            {
                barr[i].ShouldBe(false);
            }
        }

        [TestMethod]
        public void BitArrayPoolSimple2()
        {
            var barr = new BitArray64(64);

            barr[63] = true;

            barr[63].ShouldBe(true);

            for(int i = 0; i < 63; i++)
            {
                barr[i].ShouldBe(false);
            }
        }

        [TestMethod]
        public void BitArraySetIndividualBit()
        {
            for(int i = 0; i < 128; i++)
            {
                var barr = new BitArray64(128);
                barr[i] = true;

                for(int j = 0; j < 128; j++)
                {
                    bool expected = i == j;

                    barr[j].ShouldBe(expected, $"{i}");
                }
            }
        }

        [TestMethod]
        public void BitArrayClearIndividualBit()
        {
            for(int i = 0; i < 128; i++)
            {
                var barr = new BitArray64(128, true);

                barr[i] = false;

                for(int j = 0; j < 128; j++)
                {
                    bool expected = i != j;

                    barr[j].ShouldBe(expected, $"{i}");
                }
            }
        }

        [TestMethod]
        public void BitArraySpecifyPool()
        {
            var pool = ArrayPool<ulong>.Create();

            var barr = new BitArray64(1048577, pool);

            barr[91283] = true;
            barr[1048575] = true;

            for(int i = 0; i < 1048577; i++)
            {
                bool expected = (i == 91283 || i == 1048575);

                barr[i].ShouldBe(expected);
            }
        }

        [TestMethod]
        public void BitArrayReallocate()
        {
            var barr = new BitArray64(100, false);

            barr[99] = true;

            barr.Resize(120, forceReallocation: true);

            barr.Length.ShouldBe(120);
            barr[99].ShouldBeTrue();
        }

        [TestMethod]
        public void BitArrayRangeTooLow()
        {
            var barr = new BitArray64(64);

            Should.Throw(() =>
            {
                bool dummy = barr[-1];
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Index must be in range 0..length-1 (Parameter 'index')");
        }

        [TestMethod]
        public void BitArrayRangeTooHigh()
        {
            var barr = new BitArray64(60);

            barr[59].ShouldBe(false);

            Should.Throw(() =>
            {
                bool dummy = barr[60];
            }, typeof(ArgumentOutOfRangeException))
                .Message.ShouldBe("Index must be in range 0..length-1 (Parameter 'index')");
        }

        //[TestMethod]
        //public void BitArrayPoolFindA()
        //{
        //    var barr = new BitArray64(64);

        //    barr[0] = true;
        //    barr[4] = true;

        //    barr.IndexOf(true, 0).ShouldBe(0);
        //    barr.IndexOf(true, 1).ShouldBe(4);
        //    barr.IndexOf(true, 4).ShouldBe(4);
        //    barr.IndexOf(true, 5).ShouldBe(-1);
        //}

        //[TestMethod]
        //public void BitArrayPoolFindSkip()
        //{
        //    var barr = new BitArray64(65);

        //    barr[64] = true;

        //    barr.IndexOf(true, 0).ShouldBe(64);
        //    barr.IndexOf(true, 64).ShouldBe(64);
        //}

        //[TestMethod]
        //public void BitArrayPoolFindMid()
        //{
        //    var barr = new BitArray64(129);

        //    barr[1] = true;
        //    barr[128] = true;

        //    barr.IndexOf(true, 2).ShouldBe(128);
        //}

    }
}

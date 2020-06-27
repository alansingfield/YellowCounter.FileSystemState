using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.Bits;

namespace PathReduxTests.Bits
{
    [TestClass]
    public class BitArraySpanTests
    {
        [TestMethod]
        public void BitArraySpanCreate()
        {
            var x = new BitArray(new[] { false, true, true, false, true });

            var s = x.ToSpan(1);

            s[0].ShouldBe(true);
            s[1].ShouldBe(true);
            s[2].ShouldBe(false);
            s[3].ShouldBe(true);

        }

        [TestMethod]
        public void BitArraySpanOutOfRange()
        {
            Should.Throw(() =>
            {
                var x = new BitArray(new[] { false, true, true, false, true });
                var s = x.ToSpan(1);
                var q = s[4];
            }, typeof(IndexOutOfRangeException));

        }

        [TestMethod]
        public void BitArraySpanWrite()
        {
            var x = new BitArray(3);

            var s = x.ToSpan(1);
            s[0] = true;

            x[0].ShouldBe(false);
            x[1].ShouldBe(true);
            x[2].ShouldBe(false);

        }
    }
}

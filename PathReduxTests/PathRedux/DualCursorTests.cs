using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTestCoder.Shouldly.Gen;
using YellowCounter.FileSystemState.HashedStorage;

namespace PathReduxTests.PathRedux
{
    [TestClass]
    public class DualCursorTests
    {
        [TestMethod]
        public void DualCursorSequence()
        {
            var dc = new DualCursor(
                capacity: 6,
                startIndexA: 0,
                startIndexB: 3,
                probeLimitA: 3,
                probeLimitB: 3);

            var indices = new int[6];

            for(int i = 0; i < 6; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(6);
                indices[0].ShouldBe(0);
                indices[1].ShouldBe(3);
                indices[2].ShouldBe(1);
                indices[3].ShouldBe(4);
                indices[4].ShouldBe(2);
                indices[5].ShouldBe(5);
            }
        }

        [TestMethod]
        public void DualCursorOverlap()
        {
            var dc = new DualCursor(
                capacity: 6,
                startIndexA: 0,
                startIndexB: 2,
                probeLimitA: 6,
                probeLimitB: 6);

            var indices = new int[6];

            for(int i = 0; i < 6; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();
            dc.Ended.ShouldBeTrue();

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(6);
                indices[0].ShouldBe(0);
                indices[1].ShouldBe(2);
                indices[2].ShouldBe(1);
                indices[3].ShouldBe(3);
                indices[4].ShouldBe(4);
                indices[5].ShouldBe(5);
            }
        }

        [TestMethod]
        public void DualCursorOverlapReverse()
        {
            var dc = new DualCursor(
                capacity: 6,
                startIndexA: 2,
                startIndexB: 0,
                probeLimitA: 6,
                probeLimitB: 6);

            var indices = new int[6];

            for(int i = 0; i < 6; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();
            dc.Ended.ShouldBeTrue();

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(6);
                indices[0].ShouldBe(2);
                indices[1].ShouldBe(0);
                indices[2].ShouldBe(3);
                indices[3].ShouldBe(1);
                indices[4].ShouldBe(4);
                indices[5].ShouldBe(5);
            }
        }
    }
}

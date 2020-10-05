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
                probeLimit: 6);

            var indices = new int[6];

            for(int i = 0; i < 6; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();

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
                probeLimit: 6);

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
                probeLimit: 6);

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


        [TestMethod]
        public void DualCursorProbeLimitOddA()
        {
            var dc = new DualCursor(
                capacity: 6,
                startIndexA: 0,
                startIndexB: 3,
                probeLimit: 3);

            var indices = new int[3];

            for(int i = 0; i < 3; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(3);
                indices[0].ShouldBe(0);
                indices[1].ShouldBe(3);
                indices[2].ShouldBe(1);
            }
        }

        [TestMethod]
        public void DualCursorProbeLimitOddB()
        {
            var dc = new DualCursor(
                capacity: 6,
                startIndexA: 3,
                startIndexB: 0,
                probeLimit: 3);

            var indices = new int[3];

            for(int i = 0; i < 3; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(3);
                indices[0].ShouldBe(3);
                indices[1].ShouldBe(0);
                indices[2].ShouldBe(4);
            }
        }


        [TestMethod]
        public void DualCursorProbeLimitOverlapA()
        {
            var dc = new DualCursor(
                capacity: 6,
                startIndexA: 0,
                startIndexB: 2,
                probeLimit: 5);

            var indices = new int[5];

            for(int i = 0; i < 5; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(5);
                indices[0].ShouldBe(0);
                indices[1].ShouldBe(2);
                indices[2].ShouldBe(1);
                indices[3].ShouldBe(3);
                indices[4].ShouldBe(4);
            }
        }

        [TestMethod]
        public void DualCursorProbeLimitOverlapB()
        {
            var dc = new DualCursor(
                capacity: 8,
                startIndexA: 2,
                startIndexB: 0,
                probeLimit: 6);

            var indices = new int[6];

            for(int i = 0; i < 6; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();

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


        [TestMethod]
        public void DualCursorProbeLimitOverlapWraparound()
        {
            var dc = new DualCursor(
                capacity: 8,
                startIndexA: 6,
                startIndexB: 1,
                probeLimit: 8);

            var indices = new int[8];

            for(int i = 0; i < 8; i++)
            {
                dc.MoveNext(); //.ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(8);
                indices[0].ShouldBe(6);
                indices[1].ShouldBe(1);
                indices[2].ShouldBe(7);
                indices[3].ShouldBe(2);
                indices[4].ShouldBe(0);
                indices[5].ShouldBe(3);
                indices[6].ShouldBe(4);
                indices[7].ShouldBe(5);
            }
        }




        [TestMethod]
        public void DualCursorProbeLimitEqual()
        {
            var dc = new DualCursor(
                capacity: 7,
                startIndexA: 4,
                startIndexB: 4,
                probeLimit: 5);

            var indices = new int[5];

            for(int i = 0; i < 5; i++)
            {
                dc.MoveNext().ShouldBeTrue();
                indices[i] = dc.Index;
            }

            dc.MoveNext().ShouldBeFalse();

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(5);
                indices[0].ShouldBe(4);
                indices[1].ShouldBe(5);
                indices[2].ShouldBe(6);
                indices[3].ShouldBe(0);
                indices[4].ShouldBe(1);
            }
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using UnitTestCoder.Shouldly.Gen;
using YellowCounter.FileSystemState.HashedStorage;

namespace PathReduxTests.PathRedux
{
    [TestClass]
    public class CursorTests
    {
        [TestMethod]
        public void CursorInitialPositionZero()
        {
            var cursor = new Cursor(0, 3, 3);

            ShouldlyTest.Gen(cursor, nameof(cursor));

            {
                cursor.ShouldNotBeNull();
                cursor.Started.ShouldBe(false);
                cursor.Ended.ShouldBe(false);
                cursor.Index.ShouldBe(0);
                cursor.MoveCount.ShouldBe(0);
                cursor.StartIndex.ShouldBe(0);
                cursor.Capacity.ShouldBe(3);
                cursor.MoveLimit.ShouldBe(3);
            }
        }

        [TestMethod]
        public void CursorInitialPositionNonZero()
        {
            var cursor = new Cursor(2, 3, 3);

            //ShouldlyTest.Gen(cursor, nameof(cursor));

            {
                cursor.ShouldNotBeNull();
                cursor.Started.ShouldBe(false);
                cursor.Ended.ShouldBe(false);
                cursor.Index.ShouldBe(2);
                cursor.MoveCount.ShouldBe(0);
                cursor.StartIndex.ShouldBe(2);
                cursor.Capacity.ShouldBe(3);
                cursor.MoveLimit.ShouldBe(3);
            }
        }

        [TestMethod]
        public void CursorMoveNextInitial()
        {
            var cursor = new Cursor(0, 3, 3);

            cursor.MoveNext().ShouldBeTrue();

            //ShouldlyTest.Gen(cursor, nameof(cursor));

            {
                cursor.ShouldNotBeNull();
                cursor.Started.ShouldBe(true);
                cursor.Ended.ShouldBe(false);
                cursor.Index.ShouldBe(0);
                cursor.MoveCount.ShouldBe(1);
                cursor.StartIndex.ShouldBe(0);
                cursor.Capacity.ShouldBe(3);
                cursor.MoveLimit.ShouldBe(3);
            }
        }

        [TestMethod]
        public void CursorMoveNextStartAtEnd()
        {
            var cursor = new Cursor(2, 3, 3);

            cursor.MoveNext().ShouldBeTrue();

            //ShouldlyTest.Gen(cursor, nameof(cursor));

            {
                cursor.ShouldNotBeNull();
                cursor.Started.ShouldBe(true);
                cursor.Ended.ShouldBe(false);
                cursor.Index.ShouldBe(2);
                cursor.MoveCount.ShouldBe(1);
                cursor.StartIndex.ShouldBe(2);
                cursor.Capacity.ShouldBe(3);
                cursor.MoveLimit.ShouldBe(3);
            }
        }

        [TestMethod]
        public void CursorMoveNextWraparound()
        {
            var cursor = new Cursor(2, 3, 3);

            cursor.MoveNext().ShouldBeTrue();
            cursor.MoveNext().ShouldBeTrue();

            //ShouldlyTest.Gen(cursor, nameof(cursor));

            {
                cursor.ShouldNotBeNull();
                cursor.Started.ShouldBe(true);
                cursor.Ended.ShouldBe(false);
                cursor.Index.ShouldBe(0);
                cursor.MoveCount.ShouldBe(2);
                cursor.StartIndex.ShouldBe(2);
                cursor.Capacity.ShouldBe(3);
                cursor.MoveLimit.ShouldBe(3);
            }
        }

        [TestMethod]
        public void CursorMoveNextExhaust()
        {
            var cursor = new Cursor(2, 3, 3);

            // MoveLimit is 3, so we can do 3x calls to MoveNext successfully
            // The 4th call will return false, and set the Ended flag.
            // The cursor remains on the last item.
            cursor.MoveNext().ShouldBeTrue();
            cursor.MoveNext().ShouldBeTrue();
            cursor.MoveNext().ShouldBeTrue();
            cursor.Index.ShouldBe(1);

            cursor.MoveNext().ShouldBeFalse();

            //ShouldlyTest.Gen(cursor, nameof(cursor));

            {
                cursor.ShouldNotBeNull();
                cursor.Started.ShouldBe(true);
                cursor.Ended.ShouldBe(true);
                cursor.Index.ShouldBe(1);
                cursor.MoveCount.ShouldBe(3);
                cursor.StartIndex.ShouldBe(2);
                cursor.Capacity.ShouldBe(3);
                cursor.MoveLimit.ShouldBe(3);
            }
        }

    }
}

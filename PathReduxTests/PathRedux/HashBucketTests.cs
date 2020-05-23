using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.PathRedux;
using Shouldly;
using System.Linq;

namespace PathReduxTests.PathRedux
{
    [TestClass]
    public class HashBucketTests
    {
        [TestMethod]
        public void HashBucketStoreRetrieve()
        {
            var m = new HashBucket<int>(2, 2);

            m.Store(0, 123456).ShouldBe(true);
            m.Store(0, 765432).ShouldBe(true);

            var result = m.Retrieve(0);

            result.ToArray().ShouldBe(new[] { 123456, 765432 });
        }

        [TestMethod]
        public void HashBucketStoreFlowpast()
        {
            var m = new HashBucket<int>(2, 2);

            m.Store(1, 123456).ShouldBe(true);
            m.Store(1, 765432).ShouldBe(true);

            var result = m.Retrieve(1);

            result.ToArray().ShouldBe(new[] { 123456, 765432 });
        }

        [TestMethod]
        public void HashBucketStoreZero()
        {
            var m = new HashBucket<int>(2, 2);

            // It can store a zero
            m.Store(0, 0).ShouldBe(true);

            var result = m.Retrieve(0);
            result.ToArray().ShouldBe(new[] { 0 });
        }

        [TestMethod]
        public void HashBucketChainLimit()
        {
            var m = new HashBucket<int>(8, 2);

            m.Store(0, 100).ShouldBe(true);
            m.Store(0, 200).ShouldBe(true);
            m.Store(0, 300).ShouldBe(false);

            var result = m.Retrieve(0);

            result.ToArray().ShouldBe(new[] { 100, 200 });
        }

        [TestMethod]
        public void HashBucketOverlap()
        {
            var m = new HashBucket<int>(8, 8);

            // The values are going to overlap.
            m.Store(0, 100).ShouldBe(true);
            m.Store(1, 200).ShouldBe(true);
            m.Store(0, 300).ShouldBe(true);

            var result = m.Retrieve(0);

            result.ToArray().ShouldBe(new[] { 100, 200, 300 });
        }

        [TestMethod]
        public void HashBucketOverlapLimited()
        {
            var m = new HashBucket<int>(8, 2);

            // If we set the max chain to a lower value then the overlap
            // won't occur.
            m.Store(0, 100).ShouldBe(true);
            m.Store(1, 200).ShouldBe(true);
            m.Store(0, 300).ShouldBe(false);

            m.MaxLinearSearch.ShouldBe(1);

            // Because max linear search encountered is 1, we don't get
            // the value 200 back on the first row.
            m.Retrieve(0).ToArray().ShouldBe(new[] { 100 });
            m.Retrieve(1).ToArray().ShouldBe(new[] { 200 });
        }

        [TestMethod]
        public void HashBucketOverlapLimited2()
        {
            var m = new HashBucket<int>(8, 2);

            m.Store(0, 100).ShouldBe(true);
            m.Store(1, 200).ShouldBe(true);

            m.Store(2, 300).ShouldBe(true);
            m.Store(2, 400).ShouldBe(true);

            m.MaxLinearSearch.ShouldBe(2);

            m.Retrieve(0).ToArray().ShouldBe(new[] { 100, 200 });
            m.Retrieve(1).ToArray().ShouldBe(new[] { 200, 300 });
            m.Retrieve(2).ToArray().ShouldBe(new[] { 300, 400 });
        }

        [TestMethod]
        public void HashBucketWraparound()
        {
            var m = new HashBucket<int>(4, 2);

            m.Store(3, 100).ShouldBe(true);
            m.Store(3, 200).ShouldBe(true);

            m.Retrieve(3).ToArray().ShouldBe(new[] { 100, 200 });
        }

        [TestMethod]
        public void HashBucketRebuild1()
        {
            var m = new HashBucket<int>(2, 2);

            var data = new[]
            {
                new { H = 7, V = 1000 },
                new { H = 13, V = 2000 },
                new { H = 19, V = 3000 },
            };

            var store1 = new List<bool>();

            foreach(var itm in data)
            {
                store1.Add(m.Store(itm.H, itm.V));
            }

            // Both successes should end up in bucket 1 because modulo 2.
            m.Retrieve(1).ToArray().ShouldBe(new[] { 1000, 2000 });

            store1[0].ShouldBeTrue();
            store1[1].ShouldBeTrue();
            store1[2].ShouldBeFalse();  // run out of space

            // Rebuild
            var newBucket = m.Rebuild(data.Select(x => (x.H, x.V)).AsEnumerable());

            newBucket.Capacity.ShouldBe(3);

            newBucket.Retrieve(7).ToArray().ShouldBe(new[] { 1000, 2000, 3000 });
            newBucket.Retrieve(13).ToArray().ShouldBe(new[] { 1000, 2000, 3000 });
            newBucket.Retrieve(19).ToArray().ShouldBe(new[] { 1000, 2000, 3000 });
        }
    }
}

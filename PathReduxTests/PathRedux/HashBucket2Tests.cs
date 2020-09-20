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
    public class HashBucket2Tests
    {
        [TestMethod]
        public void HashBucket2StorageExactCapacity()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 4
            });

            hb.TryStore(0, 123m).ShouldBe(true);
            hb.TryStore(0, 456m).ShouldBe(true);
            hb.TryStore(0, 789m).ShouldBe(true);
            hb.TryStore(0, 516m).ShouldBe(true);

            var result = hb.ToList();

            //ShouldlyTest.Gen(result, nameof(result));

            result.ShouldNotBeNull();
            result.Count().ShouldBe(4);
            result[0].ShouldBe(123m);
            result[1].ShouldBe(456m);
            result[2].ShouldBe(789m);
            result[3].ShouldBe(516m);
        }

        [TestMethod]
        public void HashBucket2StorageWraparound()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 4
            });

            hb.TryStore(2, 111m).ShouldBe(true);
            hb.TryStore(2, 222m).ShouldBe(true);
            hb.TryStore(2, 333m).ShouldBe(true);
            hb.TryStore(2, 444m).ShouldBe(true);

            var result = hb.ToList();

            //ShouldlyTest.Gen(result, nameof(result));

            result.ShouldNotBeNull();
            result.Count().ShouldBe(4);
            result[0].ShouldBe(333m);
            result[1].ShouldBe(444m);
            result[2].ShouldBe(111m);
            result[3].ShouldBe(222m);
        }

        [TestMethod]
        public void HashBucket2StorageOverCapacity()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 4
            });

            hb.TryStore(0, 123m).ShouldBe(true);
            hb.TryStore(0, 456m).ShouldBe(true);
            hb.TryStore(0, 789m).ShouldBe(true);
            hb.TryStore(0, 516m).ShouldBe(true);
            hb.TryStore(0, 99999m).ShouldBe(false);
        }

        [TestMethod]
        public void HashBucket2StorageTooManyHashCollisions()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 2
            });

            hb.TryStore(1, 123m).ShouldBe(true);
            hb.TryStore(1, 456m).ShouldBe(true);

            // The third number we store against hash 1 should fail because
            // the linear search limit is 2.
            hb.TryStore(1, 789m).ShouldBe(false);

            // But we should be able to store these at hash 3
            hb.TryStore(3, 516m).ShouldBe(true);
            hb.TryStore(3, 99999m).ShouldBe(true);
        }

        [TestMethod]
        public void HashBucket2RetrievalWraparound()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 2
            });

            // Because this is a bucket we can store multiple items against
            // the same hash.
            hb.TryStore(3, 111m).ShouldBe(true);
            hb.TryStore(3, 222m).ShouldBe(true);

            // The actual stored positions should wrap around to the start.
            hb[3].ShouldBe(111m);
            hb[0].ShouldBe(222m);

            var result = hb.ToArray();

            // Elements will come back in positional order so 222 will be
            // before 111.
            result.Length.ShouldBe(2);
            result[0].ShouldBe(222m);
            result[1].ShouldBe(111m);
        }

        [TestMethod]
        public void HashBucket2Delete()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 4
            });

            var indices = new int[4];

            hb.TryStore(3, 111m, out indices[0]).ShouldBe(true);
            hb.TryStore(3, 222m, out indices[1]).ShouldBe(true);

            indices[0].ShouldBe(3);
            indices[1].ShouldBe(0);

            var before = hb.ToArray();

            hb.DeleteAt(3);

            var after = hb.ToArray();

            var result = new
            {
                Before = before,
                After = after
            };

            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldNotBeNull();
                result.Before.ShouldNotBeNull();
                result.Before.Count().ShouldBe(2);
                result.Before[0].ShouldBe(222m);
                result.Before[1].ShouldBe(111m);
                result.After.ShouldNotBeNull();
                result.After.Count().ShouldBe(1);
                result.After[0].ShouldBe(222m);
            }
        }

        [TestMethod]
        public void HashBucket2RetrieveByRef()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 4
            });

            var indices = new int[4];

            hb.TryStore(3, 111m, out indices[0]).ShouldBe(true);
            hb.TryStore(3, 222m, out indices[1]).ShouldBe(true);

            int iter = 0;
            foreach(ref var itm in hb.Retrieve(3))
            {
                // Amend the first item we find in-situ
                if(iter == 0)
                {
                    itm.ShouldBe(111m);
                    itm = 999m;         // IN-PLACE AMENDMENT
                }

                iter++;
            }

            // Verify the whole contents. Note that because of wraparound
            // we get the element with 222 in it first, then the element
            // we changed.
            var result = hb.ToArray();

            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldNotBeNull();
                result.Count().ShouldBe(2);
                result[0].ShouldBe(222m);
                result[1].ShouldBe(999m);
            }
        }

        [TestMethod]
        public void HashBucket2DeleteByRef()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 4
            });

            var indices = new int[4];

            hb.TryStore(3, 111m, out indices[0]).ShouldBe(true);
            hb.TryStore(3, 222m, out indices[1]).ShouldBe(true);

            int iter = 0;
            foreach(ref var itm in hb.Retrieve(3))
            {
                // Delete the first item we find in-situ
                if(iter == 0)
                {
                    itm.ShouldBe(111m);

                    // We are allowed to delete in-situ, while iterating!
                    // Try that with a normal C# list...
                    hb.Delete(ref itm);
                }

                iter++;
            }

            // Verify the whole contents. 
            var result = hb.ToArray();

            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldNotBeNull();
                result.Count().ShouldBe(1);
                result[0].ShouldBe(222m);
            }
        }


        [TestMethod]
        public void HashBucket2DeleteWithGap()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 4
            });

            var indices = new int[4];

            hb.TryStore(0, 111m, out indices[0]).ShouldBe(true);
            hb.TryStore(0, 222m, out indices[1]).ShouldBe(true);
            hb.TryStore(0, 333m, out indices[2]).ShouldBe(true);

            indices[0].ShouldBe(0);
            indices[1].ShouldBe(1);
            indices[2].ShouldBe(2);

            hb.DeleteAt(indices[1]);

            // Verify the whole contents. 
            var result = hb.ToArray();

            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldNotBeNull();
                result.Count().ShouldBe(2);
                result[0].ShouldBe(111m);
                result[1].ShouldBe(333m);
            }
        }

    }
}

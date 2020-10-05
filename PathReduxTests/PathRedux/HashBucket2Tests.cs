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
                //LinearSearchLimit = 4
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
                //LinearSearchLimit = 4
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
                //LinearSearchLimit = 4
            });

            hb.TryStore(0, 123m).ShouldBe(true);
            hb.TryStore(0, 456m).ShouldBe(true);
            hb.TryStore(0, 789m).ShouldBe(true);
            hb.TryStore(0, 516m).ShouldBe(true);
            hb.TryStore(0, 99999m).ShouldBe(false);
        }

        //[TestMethod]
        //public void HashBucket2StorageTooManyHashCollisions()
        //{
        //    var hb = new HashBucket2<decimal>(new HashBucket2Options()
        //    {
        //        Capacity = 4,
        //        //LinearSearchLimit = 2
        //    });

        //    hb.TryStore(1, 123m).ShouldBe(true);
        //    hb.TryStore(1, 456m).ShouldBe(true);

        //    // The third number we store against hash 1 should fail because
        //    // the linear search limit is 2.
        //    hb.TryStore(1, 789m).ShouldBe(false);

        //    // But we should be able to store these at hash 3
        //    hb.TryStore(3, 516m).ShouldBe(true);
        //    hb.TryStore(3, 99999m).ShouldBe(true);
        //}

        [TestMethod]
        public void HashBucket2RetrievalWraparound()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                //LinearSearchLimit = 2
            });

            // Because this is a bucket we can store multiple items against
            // the same hash.
            hb.TryStore(3, 111m).ShouldBe(true);
            hb.TryStore(3, 222m).ShouldBe(true);

            //// The actual stored positions should wrap around to the start.
            //hb[3].ShouldBe(111m);
            //hb[0].ShouldBe(222m);

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
                //LinearSearchLimit = 4
            });

            var indices = new int[4];

            hb.TryStore(3, 111m, out indices[0]).ShouldBe(true);
            hb.TryStore(3, 222m, out indices[1]).ShouldBe(true);

            //indices[0].ShouldBe(3);
            //indices[1].ShouldBe(0);

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
                //LinearSearchLimit = 4
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
                //LinearSearchLimit = 4
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
                //LinearSearchLimit = 4
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


        [TestMethod]
        public void HashBucket2ChunkLimitNoDualHash()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 8,
                ChunkSize = 2,
                Permute = x => x        // Don't do dual hashing
            });

            var indices = new int[8];

            hb.TryStore(0, 100m, out indices[0]).ShouldBe(true);
            hb.TryStore(0, 101m, out indices[1]).ShouldBe(true);
            hb.TryStore(0, 102m, out indices[2]).ShouldBe(true);
            hb.TryStore(0, 103m, out indices[3]).ShouldBe(true);
            hb.TryStore(6, 104m, out indices[4]).ShouldBe(true);
            hb.TryStore(6, 105m, out indices[5]).ShouldBe(true);
            hb.TryStore(6, 106m, out indices[6]).ShouldBe(true);
            hb.TryStore(6, 107m, out indices[7]).ShouldBe(true);
            
            hb.TryStore(0, 108m).ShouldBe(false);

            //ShouldlyTest.Gen(indices, nameof(indices));

            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(8);
                indices[0].ShouldBe(0);
                indices[1].ShouldBe(1);
                indices[2].ShouldBe(2);
                indices[3].ShouldBe(3);
                indices[4].ShouldBe(6);
                indices[5].ShouldBe(7);
                indices[6].ShouldBe(4);
                indices[7].ShouldBe(5);
            }

            var result = hb.ToList();

            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldNotBeNull();
                result.Count().ShouldBe(8);
                result[0].ShouldBe(100m);
                result[1].ShouldBe(101m);
                result[2].ShouldBe(102m);
                result[3].ShouldBe(103m);
                result[4].ShouldBe(106m);
                result[5].ShouldBe(107m);
                result[6].ShouldBe(104m);
                result[7].ShouldBe(105m);
            }
        }

        [TestMethod]
        public void HashBucket2ChunkLimitPermute()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 8,
                ChunkSize = 2,
                Permute = x => 7 - x
            });

            var indices = new int[8];

            hb.TryStore(0, 100m, out indices[0]).ShouldBe(true);
            hb.TryStore(0, 101m, out indices[1]).ShouldBe(true);
            hb.TryStore(0, 102m, out indices[2]).ShouldBe(true);
            hb.TryStore(0, 103m, out indices[3]).ShouldBe(true);
            hb.TryStore(6, 104m, out indices[4]).ShouldBe(true);
            hb.TryStore(6, 105m, out indices[5]).ShouldBe(true);
            hb.TryStore(6, 106m, out indices[6]).ShouldBe(true);
            hb.TryStore(6, 107m, out indices[7]).ShouldBe(true);

            hb.TryStore(0, 108m).ShouldBe(false);

            //ShouldlyTest.Gen(indices, nameof(indices));
            
            {
                indices.ShouldNotBeNull();
                indices.Count().ShouldBe(8);
                indices[0].ShouldBe(0);
                indices[1].ShouldBe(7);
                indices[2].ShouldBe(1);
                indices[3].ShouldBe(2);
                indices[4].ShouldBe(6);
                indices[5].ShouldBe(3);
                indices[6].ShouldBe(4);
                indices[7].ShouldBe(5);
            }

            var result = hb.ToList();

            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldNotBeNull();
                result.Count().ShouldBe(8);
                result[0].ShouldBe(100m);
                result[1].ShouldBe(102m);
                result[2].ShouldBe(103m);
                result[3].ShouldBe(105m);
                result[4].ShouldBe(106m);
                result[5].ShouldBe(107m);
                result[6].ShouldBe(104m);
                result[7].ShouldBe(101m);
            }

            // For hash zero, we've used the first 4 possible slots
            hb.ProbeDepth(0).ShouldBe(4);

            // Which means for hash 6 it has to probe the remainder.
            // (worst case scenario)
            hb.ProbeDepth(6).ShouldBe(8);
        }


    }
}

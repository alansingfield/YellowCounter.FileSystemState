using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnitTestCoder.Shouldly.Gen;
using YellowCounter.FileSystemState.HashedStorage;

namespace PathReduxTests.PathRedux
{
    [TestClass]
    public class HashBucketTests
    {
        [TestMethod]
        public void HashBucketStorageExactCapacity()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketStorageWraparound()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketStorageOverCapacity()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        //public void HashBucketStorageTooManyHashCollisions()
        //{
        //    var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketRetrievalWraparound()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketDelete()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketRetrieveByRef()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketDeleteByRef()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketDeleteWithGap()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketChunkLimitNoDualHash()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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
        public void HashBucketChunkLimitPermute()
        {
            var hb = new HashBucket<decimal>(new HashBucketOptions()
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


        [TestMethod]
        public void HashBucketRandomDistribution()
        {
            var random = new Random(Seed: 12345);

            var hb = new HashBucket<int>(new HashBucketOptions()
            {
                Capacity = 2000,
                ChunkSize = 200,
            });

            var testpoints = new int[] { 500, 1000, 1300, 1400, 1500, 1750, 1800, 1900, 2000 };

            var result = new List<string>();
            result.AddRange(new[]
            {
                "|Usage   |        |        |        |        |        |        |        |        |        |        |        |",
                "|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|",
            });

            for(int i = 0; i < hb.Capacity; i++)
            {
                int hash = random.Next(2000);

                hb.TryStore(hash, i).ShouldBeTrue();

                if(testpoints.Contains(hb.Usage))
                {
                    var probeAverage = Enumerable
                        .Range(0, 10)
                        .Average(x => (double)hb.ProbeDepth(x * hb.ChunkSize));

                    result.Add(
                        
                        String.Join("|",
                        new[]
                        {
                            "",
                            $"{hb.Usage,8}"
                        }.Concat(
                            Enumerable.Range(0, 10)
                                .Select(x => $"{hb.ProbeDepth(x * hb.ChunkSize),8}")
                        )
                        .Concat(new [] {
                            $"{probeAverage,8:F2}",
                            ""
                            })
                        ));
                }
            }

            // This demonstrates how the probe depth increases dramatically as you approach
            // 100% fill. However, the chunking cuts down on the maximum probe depth in many
            // cases.
            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldBe(new[] {
                    "|Usage   |        |        |        |        |        |        |        |        |        |        |        |",
                    "|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|",
                    "|     500|       3|       4|       4|       5|       3|       3|       3|       3|       3|       3|    3.40|",
                    "|    1000|       5|      14|       6|       6|       8|       7|       4|       6|       7|       5|    6.80|",
                    "|    1300|       8|      14|       8|       7|       8|      12|      14|      14|       9|       8|   10.20|",
                    "|    1400|      18|      14|      16|       7|      20|      12|      14|      14|      10|      23|   14.80|",
                    "|    1500|      24|      15|      25|      13|      22|      12|      14|      20|      12|      23|   18.00|",
                    "|    1750|      31|      52|      52|      23|      43|      52|      23|      31|      23|      30|   36.00|",
                    "|    1800|      48|      52|      52|      26|      43|      52|      50|      31|      23|      30|   40.70|",
                    "|    1900|     161|     110|      64|      89|      91|      52|      50|      64|      23|      57|   76.10|",
                    "|    2000|    1821|    1170|    1055|    1170|    1096|     169|    1171|     187|     233|     331|  840.30|",
                });
            }
        }

        [TestMethod]
        public void HashBucketRandomDistributionWithoutDualHashing()
        {
            var random = new Random(Seed: 12345);

            var hb = new HashBucket<int>(new HashBucketOptions()
            {
                Capacity = 2000,
                ChunkSize = 200,
                Permute = x => x,       // Don't use the second hashing.
            });

            var testpoints = new int[] { 500, 1000, 1300, 1400, 1500, 1750, 1800, 1900, 2000 };

            var result = new List<string>();
            result.AddRange(new[]
            {
                "|Usage   |        |        |        |        |        |        |        |        |        |        |        |",
                "|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|",
            });

            for(int i = 0; i < hb.Capacity; i++)
            {
                int hash = random.Next(2000);

                hb.TryStore(hash, i).ShouldBeTrue();

                if(testpoints.Contains(hb.Usage))
                {
                    var probeAverage = Enumerable
                        .Range(0, 10)
                        .Average(x => (double)hb.ProbeDepth(x * hb.ChunkSize));



                    result.Add(

                        String.Join("|",
                        new[]
                        {
                            "",
                            $"{hb.Usage,8}"
                        }.Concat(
                            Enumerable.Range(0, 10)
                                .Select(x => $"{hb.ProbeDepth(x * hb.ChunkSize),8}")
                        )
                        .Concat(new[] {
                            $"{probeAverage,8:F2}",
                            ""
                            })
                        ));
                }
            }

            // This demonstrates how the probe depth increases dramatically as you approach
            // 100% fill. Not using the second hashing almost doubles the probe depth at 90%
            // capacity.
            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldBe(new[] {
                    "|Usage   |        |        |        |        |        |        |        |        |        |        |        |",
                    "|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|--------|",
                    "|     500|       3|       4|       4|       3|       2|       3|       2|       4|       4|       3|    3.20|",
                    "|    1000|       9|      13|      14|       9|       9|       9|       3|       6|       6|       3|    8.10|",
                    "|    1300|      15|      15|      19|       9|      10|       9|       6|      34|      22|       7|   14.60|",
                    "|    1400|      37|      30|      26|       9|      11|      11|       9|      34|      27|       7|   20.10|",
                    "|    1500|      39|      30|      26|      13|      20|      11|       9|      34|      33|      18|   23.30|",
                    "|    1750|      51|     126|      49|      40|      31|      40|      13|      37|     153|      22|   56.20|",
                    "|    1800|      51|     135|      78|      45|     132|      40|      13|      37|     153|      28|   71.20|",
                    "|    1900|     621|     606|     348|     122|     138|      87|      18|      47|     153|      48|  218.80|",
                    "|    2000|    1251|    1266|    1116|     890|     729|     251|     439|      74|     305|     166|  648.70|",
                });
            }
        }


        [TestMethod]
        public void HashBucketRandomDistributionMoreChunks()
        {
            var random = new Random(Seed: 12345);

            var hb = new HashBucket<int>(new HashBucketOptions()
            {
                Capacity = 2000,
                ChunkSize = 50,
            });

            var testpoints = new int[] { 500, 1000, 1300, 1400, 1500, 1750, 1800, 1900, 2000 };

            var result = new List<string>();
            result.AddRange(new[]
            {
                "|Usage   |Probe   |",
                "|--------|--------|",
            });

            for(int i = 0; i < hb.Capacity; i++)
            {
                int hash = random.Next(2000);

                hb.TryStore(hash, i).ShouldBeTrue();

                if(testpoints.Contains(hb.Usage))
                {
                    var probeAverage = Enumerable
                        .Range(0, 40)
                        .Average(x => (double)hb.ProbeDepth(x * hb.ChunkSize));

                    result.Add(
                        String.Join("|",
                        new[]
                        {
                            "",
                            $"{hb.Usage,8}",
                            $"{probeAverage,8:F2}",
                            ""
                        }
                        ));
                }
            }

            // Does reducing the chunk size (and increasing mem usage) give us a better distribution?
            //ShouldlyTest.Gen(result, nameof(result));
            {
                result.ShouldBe(new[] {
                    "|Usage   |Probe   |",
                    "|--------|--------|",
                    "|     500|    2.35|",
                    "|    1000|    4.50|",
                    "|    1300|    6.90|",
                    "|    1400|    9.22|",
                    "|    1500|   11.88|",
                    "|    1750|   21.12|",
                    "|    1800|   24.10|",
                    "|    1900|   43.30|",
                    "|    2000|  380.05|",
                    });
            }
        }

        [TestMethod]
        public void HashBucketRandomDistributionMoreChunksWithoutDualHashing()
        {
            var random = new Random(Seed: 12345);

            var hb = new HashBucket<int>(new HashBucketOptions()
            {
                Capacity = 2000,
                ChunkSize = 50,
                Permute = x => x,       // Don't use the second hashing.
            });

            var testpoints = new int[] { 500, 1000, 1300, 1400, 1500, 1750, 1800, 1900, 2000 };

            var result = new List<string>();
            result.AddRange(new[]
            {
                "|Usage   |Probe   |",
                "|--------|--------|",
            });

            for(int i = 0; i < hb.Capacity; i++)
            {
                int hash = random.Next(2000);

                hb.TryStore(hash, i).ShouldBeTrue();

                if(testpoints.Contains(hb.Usage))
                {
                    var probeAverage = Enumerable
                        .Range(0, 40)
                        .Average(x => (double)hb.ProbeDepth(x * hb.ChunkSize));

                    result.Add(
                        String.Join("|",
                        new[]
                        {
                            "",
                            $"{hb.Usage,8}",
                            $"{probeAverage,8:F2}",
                            ""
                        }
                        ));
                }
            }

            // Does reducing the chunk size (and increasing mem usage) give us a better distribution?
            // Here we're not using the second hashing and the probe depths are much longer.
            //ShouldlyTest.Gen(result, nameof(result));
            {
                result.ShouldBe(new[] {
                    "|Usage   |Probe   |",
                    "|--------|--------|",
                    "|     500|    2.12|",
                    "|    1000|    5.00|",
                    "|    1300|    8.68|",
                    "|    1400|   11.50|",
                    "|    1500|   14.32|",
                    "|    1750|   35.52|",
                    "|    1800|   46.58|",
                    "|    1900|  127.72|",
                    "|    2000|  415.65|",
                });
            }
        }



        [TestMethod]
        public void HashBucketRandomDistributionAverageProbe()
        {
            var random = new Random(Seed: 12345);

            var hb = new HashBucket<int>(new HashBucketOptions()
            {
                Capacity = 2000,
                ChunkSize = 50,
            });

            var testpoints = new int[] { 500, 1000, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000 };

            var result = new List<string>();
            result.AddRange(new[]
            {
                "|Usage   |Same    |Diff    |Max     |",
                "|--------|--------|--------|--------|",
            });

            for(int i = 0; i < hb.Capacity; i++)
            {
                int hash = random.Next(2000);

                hb.TryStore(hash, i).ShouldBeTrue();

                if(testpoints.Contains(hb.Usage))
                {
                    var probeMax = Enumerable
                        .Range(0, hb.Capacity / hb.ChunkSize)
                        .Average(x => (double)hb.ProbeDepth(x * hb.ChunkSize));

                    // Calculate the average probe depth.
                    // Try retrieving same items and/or a different list of items.
                    var probeAverageSame = avgProbeLength(hb, new Random(Seed: 12345), i);
                    var probeAverageDiff = avgProbeLength(hb, new Random(Seed: 99999), i);

                    result.Add(
                        String.Join("|",
                        new[]
                        {
                            "",
                            $"{hb.Usage,8}",
                            $"{probeAverageSame,8:F2}",
                            $"{probeAverageDiff,8:F2}",
                            $"{probeMax,8:F2}",
                            ""
                        }
                        ));
                }
            }

            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldBe(new[] {
                    "|Usage   |Same    |Diff    |Max     |",
                    "|--------|--------|--------|--------|",
                    "|     500|    1.52|    0.31|    2.35|",
                    "|    1000|    2.64|    1.15|    4.50|",
                    "|    1300|    4.02|    2.27|    6.90|",
                    "|    1400|    4.95|    3.06|    9.22|",
                    "|    1500|    6.28|    4.07|   11.88|",
                    "|    1600|    7.63|    5.19|   13.97|",
                    "|    1700|    9.75|    7.13|   17.02|",
                    "|    1800|   14.43|   11.46|   24.10|",
                    "|    1900|   27.79|   23.78|   43.30|",
                    "|    2000|  382.67|  380.55|  380.05|",
                });
            }
        }

        [TestMethod]
        public void HashBucketRandomDistributionAverageProbeSmallChunk()
        {
            var random = new Random(Seed: 12345);

            var hb = new HashBucket<int>(new HashBucketOptions()
            {
                Capacity = 2000,
                ChunkSize = 20,
            });

            var testpoints = new int[] { 500, 1000, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000 };

            var result = new List<string>();
            result.AddRange(new[]
            {
                "|Usage   |Same    |Diff    |Mode    |Max     |",
                "|--------|--------|--------|--------|--------|",
            });

            for(int i = 0; i < hb.Capacity; i++)
            {
                int hash = random.Next(2000);

                hb.TryStore(hash, i).ShouldBeTrue();

                if(testpoints.Contains(hb.Usage))
                {
                    var probeMax = Enumerable
                        .Range(0, hb.Capacity / hb.ChunkSize)
                        .Average(x => (double)hb.ProbeDepth(x * hb.ChunkSize));

                    var probeMode = modalProbeLength(hb, new Random(Seed: 99999), i);

                    // Calculate the average probe depth.
                    // Try retrieving same items and/or a different list of items.
                    var probeAverageSame = avgProbeLength(hb, new Random(Seed: 12345), i);
                    var probeAverageDiff = avgProbeLength(hb, new Random(Seed: 99999), i);

                    result.Add(
                        String.Join("|",
                        new[]
                        {
                            "",
                            $"{hb.Usage,8}",
                            $"{probeAverageSame,8:F2}",
                            $"{probeAverageDiff,8:F2}",
                            $"{probeMode,8:F2}",
                            $"{probeMax,8:F2}",
                            ""
                        }
                        ));
                }
            }

            //ShouldlyTest.Gen(result, nameof(result));

            {
                result.ShouldBe(new[] {
                    "|Usage   |Same    |Diff    |Mode    |Max     |",
                    "|--------|--------|--------|--------|--------|",
                    "|     500|    1.42|    0.29|    1.00|    1.65|",
                    "|    1000|    2.45|    1.06|    1.00|    3.26|",
                    "|    1300|    3.59|    2.03|    2.00|    4.88|",
                    "|    1400|    4.37|    2.70|    2.00|    6.26|",
                    "|    1500|    5.48|    3.51|    2.00|    7.67|",
                    "|    1600|    6.69|    4.54|    2.00|    9.29|",
                    "|    1700|    8.44|    6.07|    5.00|   11.49|",
                    "|    1800|   12.22|    9.53|    5.00|   16.24|",
                    "|    1900|   21.41|   17.84|    5.00|   26.42|",
                    "|    2000|  188.33|  189.84|    9.00|  188.75|",
                });
            }
        }

        private double avgProbeLength(HashBucket<int> hb, Random random, int count)
        {
            double probe = 0;

            for(int i = 0; i < count; i++)
            {
                int hash = random.Next(2000);

                foreach(var itm in hb.Retrieve(hash))
                {
                    probe++;
                }
            }

            return probe / count;
        }

        private int? modalProbeLength(HashBucket<int> hb, Random random, int count)
        {
            var probes = new int[count];

            for(int i = 0; i < count; i++)
            {
                int hash = random.Next(2000);

                foreach(var itm in hb.Retrieve(hash))
                {
                    probes[i]++;
                }
            }

            int? mode = probes
                .Where(x => x!= 0)
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count()).ThenBy(x => x.Key)
                .Select(x => (int?)x.Key)
                .FirstOrDefault();

            return mode;
        }



    }
}
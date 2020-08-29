using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTestCoder.Shouldly.Gen;
using YellowCounter.FileSystemState.PathRedux;

namespace PathReduxTests.PathRedux
{
    [TestClass]
    public class HashBucket2Tests
    {
        [TestMethod]
        public void HashBucket2StorageFull()
        {
            var hb = new HashBucket2<decimal>(new HashBucket2Options()
            {
                Capacity = 4,
                LinearSearchLimit = 4
            });

            hb.TryStore(1, 123m).ShouldBe(true);
            hb.TryStore(1, 456m).ShouldBe(true);
            hb.TryStore(1, 789m).ShouldBe(true);
            hb.TryStore(1, 516m).ShouldBe(true);

            var result = new List<decimal>();

            foreach(var itm in hb)
            {
                result.Add(itm);
            }

            ShouldlyTest.Gen(result, nameof(result));

            result.ShouldNotBeNull();
            result.Count().ShouldBe(4);
            result[0].ShouldBe(516m);
            result[1].ShouldBe(123m);
            result[2].ShouldBe(456m);
            result[3].ShouldBe(789m);
        }
    }
    }
}

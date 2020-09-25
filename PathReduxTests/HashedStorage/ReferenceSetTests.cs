using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UnitTestCoder.Core.Literal;
using UnitTestCoder.Shouldly.Gen;
using YellowCounter.FileSystemState.HashedStorage;

namespace PathReduxTests.HashedStorage
{
    [TestClass]
    public class ReferenceSetTests
    {
        private struct Abc
        {
            public Abc(int key)
            {
                this.Key = key;
                this.Value = default;
            }

            public int Key { readonly get; private set; }
            public decimal Value { get; set; }
        }

        private class AbcReferenceSet : ReferenceSet<int, Abc>
        {
            protected override int GetHashOfKey(in int key)
            {
                return key / 100;
            }

            protected override int GetKey(in Abc item) => item.Key;

            protected override bool Match(in Abc item, in int key) => item.Key == key;
        }

        [TestMethod]
        public void ReferenceSetPopulateRefs()
        {
            var refset = new AbcReferenceSet();

            ref var item0 = ref refset.GetOrAdd(101, () => new Abc(101) { Value = 1000m });
            ref var item1 = ref refset.GetOrAdd(102, () => new Abc(102) { Value = 2000m });

            int idx = 0;
            foreach(ref var itm in refset)
            {
                switch(idx)
                {
                    case 0:
                        itm.ShouldBe(item0);
                        break;
                    case 1:
                        itm.ShouldBe(item1);
                        break;
                }

                idx++;
            }
        }

        [TestMethod]
        public void ReferenceSetPopulateValues()
        {
            var refset = new AbcReferenceSet();

            refset.GetOrAdd(101, () => new Abc(101) { Value = 1000m });
            refset.GetOrAdd(102, () => new Abc(102) { Value = 2000m });

            var copy = refset.ToArray();

            //ShouldlyTest.Gen(copy, nameof(copy));

            {
                copy.ShouldNotBeNull();
                copy.Count().ShouldBe(2);
                copy[0].ShouldNotBeNull();
                copy[0].Key.ShouldBe(101);
                copy[0].Value.ShouldBe(1000m);
                copy[1].ShouldNotBeNull();
                copy[1].Key.ShouldBe(102);
                copy[1].Value.ShouldBe(2000m);
            }
        }


        [TestMethod]
        public void ReferenceSetReadByKeyRef()
        {
            var refset = new AbcReferenceSet();

            refset.GetOrAdd(101, () => new Abc(101) { Value = 1000m });
            refset.GetOrAdd(102, () => new Abc(102) { Value = 2000m });

            // Read an item by key
            ref var item101 = ref refset[101];

            item101.Value.ShouldBe(1000m);

            item101.Value = 999m;

            var copy = refset.ToArray();

            copy[0].Key.ShouldBe(101);
            copy[0].Value.ShouldBe(999m);
        }

        [TestMethod]
        public void ReferenceSetKeyMissing()
        {
            var refset = new AbcReferenceSet();

            refset.GetOrAdd(101, () => new Abc(101) { Value = 1000m });
            refset.GetOrAdd(102, () => new Abc(102) { Value = 2000m });

            Should.Throw(() =>
            {
                ref var itm = ref refset[103];
            }, typeof(ArgumentException)).Message.ShouldBe("Key was not found in the set (Parameter 'key')");
        }

        [TestMethod]
        public void ReferenceSetKeyMissingSoftDeleted()
        {
            var refset = new AbcReferenceSet();

            refset.GetOrAdd(101, () => new Abc(101) { Value = 1000m });
            ref var itm2 = ref refset.GetOrAdd(102, () => new Abc(102) { Value = 2000m });

            refset.Delete(ref itm2);

            Should.Throw(() =>
            {
                ref var itm = ref refset[102];
            }, typeof(ArgumentException)).Message.ShouldBe("Key was not found in the set (Parameter 'key')");
        }

    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PathReduxTests.RefExperiment
{
    [TestClass]
    public class RefExperimentTests
    {
        [TestMethod]
        public void RefExperiment1()
        {
            var retriever = new Retriever();

            ref var first = ref retriever.Get(1);
            first = 12;

            retriever.Rebuild();

            ref var second = ref retriever.Get(1);

            second = 14;

            first.ShouldBe(12);
            second.ShouldBe(14);

        }

        private class Retriever
        {
            private MemStore memStore = new MemStore();

            public ref int Get(int index)
            {
                return ref memStore[index];
            }

            public void Rebuild()
            {
                var newMemStore = new MemStore();

                for(int i = 0; i < 3; i++)
                {
                    newMemStore[i] = memStore[i];
                }

                this.memStore = newMemStore;
            }
        }

        private class MemStore
        {
            int[] array = new int[4];

            public ref int this[int index]
            {
                get
                {
                    return ref array[index];
                }
            }
        }


        [TestMethod]
        public void RefExperiment2()
        {
            int[] x = new int[4];

            ref var first = ref x[0];
            ref var second = ref x[1];

            int indexOffset = (int)Unsafe.ByteOffset(ref first, ref second) / Unsafe.SizeOf<int>();

            indexOffset.ShouldBe(1);
        }

        //[TestMethod]
        //public void RefExperiment3()
        //{
        //    int[] x = new int[4];
        //    int[] y = new int[4];

        //    ref var first = ref x[0];
        //    ref var second = ref y[1];

        //    int indexOffset = (int)Unsafe.ByteOffset(ref first, ref second) / Unsafe.SizeOf<int>();

        //    // Could be anything ....
        //    indexOffset.ShouldBe(11);
        //}

        //[TestMethod]
        //public void RefExperiment4()
        //{
        //    var x = new MemStore();
        //    //GC.Collect();
        //    var y = new MemStore();

        //    ref var first = ref x[0];
        //    ref var second = ref y[1];

        //    int indexOffset = (int)Unsafe.ByteOffset(ref first, ref second) / Unsafe.SizeOf<int>();

        //    // Could be anything...
        //    indexOffset.ShouldBe(17);
        //}
    }
}

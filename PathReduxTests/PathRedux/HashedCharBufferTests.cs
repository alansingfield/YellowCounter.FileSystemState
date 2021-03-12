using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using PathReduxTests.HashCodes;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState.HashedStorage;
using YellowCounter.FileSystemState.Options;
using YellowCounter.FileSystemState.PathRedux;

namespace PathReduxTests.PathRedux
{
    [TestClass]
    public class HashedCharBufferTests
    {

        [TestMethod]
        public void HashedCharBufferAddAndRetrieveNoClash()
        {
            var buf = new HashedCharBuffer(new HashedCharBufferOptions()
            {
                NewHashCode = () => new DeterministicHashCode(),
                InitialCharCapacity = 20,
                HashBucketOptions = new HashBucketOptions()
                {
                    Capacity = 16,
                }
            });

            buf.Store("Hello");
            buf.Store("World");

            buf.Find("Hello").ShouldBe(0);
            buf.Find("World").ShouldBe(6);

            buf.Retrieve(0).ToString().ShouldBe("Hello");
            buf.Retrieve(6).ToString().ShouldBe("World");
        }

        [TestMethod]
        public void HashedCharBufferAddAndRetrieveClash()
        {
            var buf = new HashedCharBuffer(new HashedCharBufferOptions()
            {
                NewHashCode = () => new ControllableHashCode(),
                InitialCharCapacity = 20,
                HashBucketOptions = new HashBucketOptions()
                {
                    Capacity = 16,
                }
            });

            buf.Store("1,Hello");
            buf.Store("1,World");

            //// Confirm that both strings the same hashcode.
            buf.HashSequence("1,Hello").ShouldBe(1);
            buf.HashSequence("1,World").ShouldBe(1);

            buf.Find("1,Hello").ShouldBe(0);
            buf.Find("1,World").ShouldBe(8);

            buf.Retrieve(0).ToString().ShouldBe("1,Hello");
            buf.Retrieve(8).ToString().ShouldBe("1,World");
        }

    }
}

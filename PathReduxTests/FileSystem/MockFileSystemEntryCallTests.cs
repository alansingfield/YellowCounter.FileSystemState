using FSSUnsafe2.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;
using Shouldly;

namespace PathReduxTests.FileSystem
{
    [TestClass]
    public class MockFileSystemEntryCallTests
    {
        [TestMethod]
        public void MockFileSystemEntryCallInOut()
        {
            var mfseTest = new MFSETest();

            mfseTest.CallIn(1,"Hello", 12345L);
        }

        private class MFSETest : MockFileSystemEntryCall<int>
        {
            protected override void CallOut(int context, ref FileSystemEntry fse)
            {
                context.ShouldBe(1);

                fse.FileName.ToString().ShouldBe("Hello");
            }
        }
    }
}

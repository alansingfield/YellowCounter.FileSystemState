using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PathReduxTests.FileSystem
{
    [TestClass]
    public class WorkingFolderTests
    {
        [TestMethod]
        public void WorkingFolderCreatesAndDestroys()
        {
            var wf = new WorkingFolder();

            Directory.Exists(wf.Folder).ShouldBe(true);

            wf.Dispose();

            Directory.Exists(wf.Folder).ShouldBe(false);
        }
    }
}

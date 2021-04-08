using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using NSubstitute;
using YellowCounter.FileSystemState;
using YellowCounter.FileSystemState.Options;

namespace PathReduxTests.FileSystem
{
    [TestClass]
    public class FileSystemChangeEnumeratorTests
    {
        [TestMethod]
        public void FileSystemChangeEnumeratorConstructs()
        {
            //var acceptFileSsytemEntry = Substitute.For<IAcceptFileSystemEntry>();

            //var options = new FileSystemStateOptions()
            //{
            //    RecurseSubdirectories = true,
            //    IgnoreInaccessible = true,
            //    AttributesToSkip = System.IO.FileAttributes.Hidden,
            //};

            //var enumerator = new FileSystemChangeEnumerator(
            //    "FakeDir",
            //    options,
            //    acceptFileSsytemEntry);

            //enumerator.
        }

    }
}

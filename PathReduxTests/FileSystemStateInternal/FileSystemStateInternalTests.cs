using DryIoc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathReduxTests.DryIoc;
using PathReduxTests.FileSystem;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;
using YellowCounter.FileSystemState;
using Shouldly;

namespace PathReduxTests.FileSystemStateInternalTest
{
    [TestClass]
    public class FileSystemStateInternalTests
    {
        [TestMethod]
        public void FileSystemStateInternalLoad()
        {
            //var container = new Container().WithNSubstituteFallback();

            //// The concrete class we are testing
            //container.Register<IFileSystemStateInternal, FileSystemStateInternal>();

            var fseCopy = new FileSystemEntryCopy();

            fseCopy.FileName = "HelloWorld";

            FileSystemEntry fse = fseCopy.ToFse();

            fse.FileName.ToString().ShouldBe("HelloWorld");
        }
    }
}

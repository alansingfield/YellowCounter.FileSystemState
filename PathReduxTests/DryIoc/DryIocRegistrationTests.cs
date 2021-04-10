using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using DryIoc;
using YellowCounter.FileSystemState;

namespace PathReduxTests.DryIoc
{
    [TestClass]
    public class DryIocRegistrationTests
    {
        [TestMethod]
        public void DryIocRegistrationDefault()
        {
            IContainer container = new Container();

            container.RegisterFileSystemState();

            container.ValidateOrThrow();
        }
    }
}

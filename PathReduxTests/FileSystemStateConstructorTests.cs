using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using YellowCounter.FileSystemState;
using YellowCounter.FileSystemState.Options;
using DryIoc;
using Shouldly;
using YellowCounter.FileSystemState.Filter;
using NSubstitute;
using System.IO;

namespace PathReduxTests
{
    [TestClass]
    public class FileSystemStateConstructorTests
    {
        [TestMethod]
        public void FileSystemStateConstructorRootFolder()
        {
            var opts = new FileSystemStateOptions();

            var fss = new FileSystemState(@"FAKE:\", opts);
            
            var context = fss.Context;

            var root = context.Resolve<IRootDir>();
            root.Folder.ShouldBe(@"FAKE:\");
        }


        [TestMethod]
        public void FileSystemStateConstructorFilterDefault()
        {
            var opts = new FileSystemStateOptions();

            var fss = new FileSystemState(@"FAKE:\", opts);

            var context = fss.Context;

            var filter = context.Resolve<IFilenameFilter>();

            // The Filter on our options should be blank.
            opts.Filter.ShouldBe(null);

            // We'll get the default filter.
            filter.ShouldNotBe(null);
            filter.ShouldBeOfType(typeof(FilenameFilter));
        }


        [TestMethod]
        public void FileSystemStateConstructorFilterOverride()
        {
            var subFilter = Substitute.For<IFilenameFilter>();

            var opts = new FileSystemStateOptions()
            {
                Filter = subFilter
            };

            var fss = new FileSystemState(@"FAKE:\", opts);

            var context = fss.Context;

            var filter = context.Resolve<IFilenameFilter>();

            // Check the filter we set in the options is pulled through.
            filter.ShouldBe(opts.Filter);
        }

        [TestMethod]
        public void FileSystemStateConstructorDirectoryFilterDefault()
        {
            var opts = new FileSystemStateOptions();

            var fss = new FileSystemState(@"FAKE:\", opts);

            var context = fss.Context;

            var filter = context.Resolve<IDirectoryFilter>();

            // The DirectoryFilter on our options should be blank.
            opts.DirectoryFilter.ShouldBe(null);

            // We'll get the default filter.
            filter.ShouldNotBe(null);
            filter.ShouldBeOfType(typeof(DirectoryFilter));
        }


        [TestMethod]
        public void FileSystemStateConstructorDirectoryFilterOverride()
        {
            var subDirectoryFilter = Substitute.For<IDirectoryFilter>();

            var opts = new FileSystemStateOptions()
            {
                DirectoryFilter = subDirectoryFilter
            };

            var fss = new FileSystemState(@"FAKE:\", opts);

            var context = fss.Context;

            var filter = context.Resolve<IDirectoryFilter>();

            // Check the filter we set in the options is pulled through.
            filter.ShouldBe(opts.DirectoryFilter);
        }



        [TestMethod]
        public void FileSystemStateConstructorEnumeratorOptionRecurseSubdirectories()
        {
            foreach(var val in new[] { false, true })
            {
                var opts = new FileSystemStateOptions()
                {
                    RecurseSubdirectories = val
                };

                var fss = new FileSystemState(@"FAKE:\", opts);

                var context = fss.Context;

                var enumOptions = context.Resolve<EnumerationOptions>();

                enumOptions.RecurseSubdirectories.ShouldBe(val);
            }
        }

        [TestMethod]
        public void FileSystemStateConstructorEnumeratorOptionIgnoreInaccessible()
        {
            foreach(var val in new[] { false, true })
            {
                var opts = new FileSystemStateOptions()
                {
                    IgnoreInaccessible = val
                };

                var fss = new FileSystemState(@"FAKE:\", opts);

                var context = fss.Context;

                var enumOptions = context.Resolve<EnumerationOptions>();

                enumOptions.IgnoreInaccessible.ShouldBe(val);
            }
        }

        [TestMethod]
        public void FileSystemStateConstructorEnumeratorOptionAttributesToSkip()
        {
            foreach(var val in new[] { 
                FileAttributes.Hidden | FileAttributes.System,
                FileAttributes.Normal 
            })
            {
                var opts = new FileSystemStateOptions()
                {
                    AttributesToSkip = val
                };

                var fss = new FileSystemState(@"FAKE:\", opts);

                var context = fss.Context;

                var enumOptions = context.Resolve<EnumerationOptions>();

                enumOptions.AttributesToSkip.ShouldBe(val);
            }
        }


        [TestMethod]
        public void FileSystemStateConstructorRootOnly()
        {
            var fss = new FileSystemState(@"FAKE:\");

            var context = fss.Context;

            var root = context.Resolve<IRootDir>();
            root.Folder.ShouldBe(@"FAKE:\");

            var filter = context.Resolve<IFilenameFilter>();

            // We'll get the default filter.
            filter.ShouldNotBe(null);
            filter.ShouldBeOfType(typeof(FilenameFilter));

            var directoryFilter = context.Resolve<IDirectoryFilter>();

            directoryFilter.ShouldNotBe(null);
            directoryFilter.ShouldBeOfType(typeof(DirectoryFilter));
        }

        [TestMethod]
        public void FileSystemStateConstructorRootAndFilter()
        {
            var fss = new FileSystemState(@"FAKE:\", "*.docx");

            var context = fss.Context;

            var root = context.Resolve<IRootDir>();
            root.Folder.ShouldBe(@"FAKE:\");

            var filter = context.Resolve<IFilenameFilter>();

            filter.ShouldNotBe(null);

            filter.ShouldInclude("abc.docx").ShouldBe(true);
            filter.ShouldInclude("cde.xlsx").ShouldBe(false);
        }

        [TestMethod]
        public void FileSystemStateConstructorRootFilterRecursive()
        {
            foreach(var val in new[] { false, true })
            {
                var fss = new FileSystemState(@"FAKE:\", recursive: val);

                var context = fss.Context;

                var enumerationOptions = context.Resolve<EnumerationOptions>();

                enumerationOptions.ShouldNotBeNull();
                enumerationOptions.RecurseSubdirectories.ShouldBe(val);
            }
        }

        [TestMethod]
        public void FileSystemStateConstructorNoRoot()
        {
            Should.Throw(() =>
            {
                new FileSystemState(null);
            }, typeof(ArgumentNullException))
                .Message.ShouldBe("Value cannot be null. (Parameter 'rootDir')");
        }

        [TestMethod]
        public void FileSystemStateConstructorNoOptions()
        {
            Should.Throw(() =>
            {
                new FileSystemState("", options:null);
            }, typeof(ArgumentNullException))
                .Message.ShouldBe("Value cannot be null. (Parameter 'options')");
        }


    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using NSubstitute;
using NS = NSubstitute;
using YellowCounter.FileSystemState;
using YellowCounter.FileSystemState.Options;
using DryIoc;
using PathReduxTests.DryIoc;
using System.IO;
using Shouldly;
using System.IO.Enumeration;
using UnitTestCoder.Shouldly.Gen;
using System.Linq;

namespace PathReduxTests.FileSystem
{
    [TestClass]
    public class FileSystemEnumeratorTests
    {
        [TestMethod]
        public void FileSystemEnumeratorSingleFile()
        {
            var container = new Container().WithNSubstituteFallback();

            container.RegisterFileSystemEnumerator();
            container.Register<EnumerationOptions>(Reuse.Singleton);

            var mockAcceptFileSystemEntry = new MockAcceptFileSystemEntry();

            container.Use<IAcceptFileSystemEntry>(mockAcceptFileSystemEntry);

            var rootDir = container.Resolve<IRootDir>();
            var enumOptions = container.Resolve<EnumerationOptions>();

            enumOptions.RecurseSubdirectories = false;

            // Create a temp folder
            using(var wf = new WorkingFolder())
            {
                rootDir.Folder.Returns(wf.Folder);

                var fsEnum = container.Resolve<IFileSystemEnumerator>();

                var tolerance = TimeSpan.FromSeconds(10);
                var utcNow = DateTimeOffset.UtcNow;

                // Create a file in our temp folder
                File.WriteAllText(Path.Combine(wf.Folder, "hello1.txt"), "Hello1");

                mockAcceptFileSystemEntry.ShouldIncludeEntries.Count.ShouldBe(0);
                mockAcceptFileSystemEntry.ShouldRecurseIntoEntries.Count.ShouldBe(0);
                mockAcceptFileSystemEntry.TransformEntries.Count.ShouldBe(0);

                // Should get one True from MoveNext() as we enumerate, then one False.
                fsEnum.MoveNext().ShouldBe(true);
                fsEnum.MoveNext().ShouldBe(false);

                //ShouldlyTest.Gen(mockAcceptFileSystemEntry, nameof(mockAcceptFileSystemEntry));

                {
                    mockAcceptFileSystemEntry.ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldIncludeEntries.ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldIncludeEntries.Count().ShouldBe(1);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].Attributes.ShouldBe(System.IO.FileAttributes.Archive);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].Directory.ShouldBe(wf.Folder);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].FileName.ShouldBe("hello1.txt");
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].IsDirectory.ShouldBe(false);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].Length.ShouldBe(6L);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries.ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries.Count().ShouldBe(0);
                    mockAcceptFileSystemEntry.TransformEntries.ShouldNotBeNull();
                    mockAcceptFileSystemEntry.TransformEntries.Count().ShouldBe(1);
                    mockAcceptFileSystemEntry.TransformEntries[0].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.TransformEntries[0].Attributes.ShouldBe(System.IO.FileAttributes.Archive);
                    mockAcceptFileSystemEntry.TransformEntries[0].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[0].Directory.ShouldBe(wf.Folder);
                    mockAcceptFileSystemEntry.TransformEntries[0].FileName.ShouldBe("hello1.txt");
                    mockAcceptFileSystemEntry.TransformEntries[0].IsDirectory.ShouldBe(false);
                    mockAcceptFileSystemEntry.TransformEntries[0].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.TransformEntries[0].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[0].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[0].Length.ShouldBe(6L);
                }


            }
        }


        [TestMethod]
        public void FileSystemEnumeratorFileAndDirectory()
        {
            var container = new Container().WithNSubstituteFallback();

            container.RegisterFileSystemEnumerator();
            container.Register<EnumerationOptions>(Reuse.Singleton);

            var mockAcceptFileSystemEntry = new MockAcceptFileSystemEntry();

            container.Use<IAcceptFileSystemEntry>(mockAcceptFileSystemEntry);

            var rootDir = container.Resolve<IRootDir>();
            var enumOptions = container.Resolve<EnumerationOptions>();

            // We need to confirm that the EnumerationOptions makes it through to the base constructor
            // parameters.
            enumOptions.RecurseSubdirectories = true;

            // Create a temp folder
            using(var wf = new WorkingFolder())
            {
                rootDir.Folder.Returns(wf.Folder);

                var fsEnum = container.Resolve<IFileSystemEnumerator>();

                var tolerance = TimeSpan.FromSeconds(10);
                var utcNow = DateTimeOffset.UtcNow;

                // Create files and subfolder
                File.WriteAllText(Path.Combine(wf.Folder, "hello1.txt"), "Hello1");
                Directory.CreateDirectory(Path.Combine(wf.Folder, "subfolder"));
                File.WriteAllText(Path.Combine(wf.Folder, "subfolder", "hello2.txt"), "Hello2");

                mockAcceptFileSystemEntry.ShouldIncludeEntries.Count.ShouldBe(0);
                mockAcceptFileSystemEntry.ShouldRecurseIntoEntries.Count.ShouldBe(0);
                mockAcceptFileSystemEntry.TransformEntries.Count.ShouldBe(0);

                // Should get 3 trues from MoveNext() as we enumerate, then one False.
                fsEnum.MoveNext().ShouldBe(true);
                fsEnum.MoveNext().ShouldBe(true);
                fsEnum.MoveNext().ShouldBe(true);
                fsEnum.MoveNext().ShouldBe(false);

                //ShouldlyTest.Gen(mockAcceptFileSystemEntry, nameof(mockAcceptFileSystemEntry));

                {
                    mockAcceptFileSystemEntry.ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldIncludeEntries.ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldIncludeEntries.Count().ShouldBe(3);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].Attributes.ShouldBe(System.IO.FileAttributes.Archive);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].Directory.ShouldBe(wf.Folder);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].FileName.ShouldBe("hello1.txt");
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].IsDirectory.ShouldBe(false);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[0].Length.ShouldBe(6L);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].Attributes.ShouldBe(System.IO.FileAttributes.Directory);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].Directory.ShouldBe(wf.Folder);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].FileName.ShouldBe("subfolder");
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].IsDirectory.ShouldBe(true);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[1].Length.ShouldBe(0L);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].Attributes.ShouldBe(System.IO.FileAttributes.Archive);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].Directory.ShouldBe(wf.Folder + @"\subfolder");
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].FileName.ShouldBe("hello2.txt");
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].IsDirectory.ShouldBe(false);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldIncludeEntries[2].Length.ShouldBe(6L);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries.ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries.Count().ShouldBe(1);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].Attributes.ShouldBe(System.IO.FileAttributes.Directory);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].Directory.ShouldBe(wf.Folder);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].FileName.ShouldBe("subfolder");
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].IsDirectory.ShouldBe(true);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.ShouldRecurseIntoEntries[0].Length.ShouldBe(0L);
                    mockAcceptFileSystemEntry.TransformEntries.ShouldNotBeNull();
                    mockAcceptFileSystemEntry.TransformEntries.Count().ShouldBe(3);
                    mockAcceptFileSystemEntry.TransformEntries[0].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.TransformEntries[0].Attributes.ShouldBe(System.IO.FileAttributes.Archive);
                    mockAcceptFileSystemEntry.TransformEntries[0].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[0].Directory.ShouldBe(wf.Folder);
                    mockAcceptFileSystemEntry.TransformEntries[0].FileName.ShouldBe("hello1.txt");
                    mockAcceptFileSystemEntry.TransformEntries[0].IsDirectory.ShouldBe(false);
                    mockAcceptFileSystemEntry.TransformEntries[0].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.TransformEntries[0].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[0].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[0].Length.ShouldBe(6L);
                    mockAcceptFileSystemEntry.TransformEntries[1].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.TransformEntries[1].Attributes.ShouldBe(System.IO.FileAttributes.Directory);
                    mockAcceptFileSystemEntry.TransformEntries[1].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[1].Directory.ShouldBe(wf.Folder);
                    mockAcceptFileSystemEntry.TransformEntries[1].FileName.ShouldBe("subfolder");
                    mockAcceptFileSystemEntry.TransformEntries[1].IsDirectory.ShouldBe(true);
                    mockAcceptFileSystemEntry.TransformEntries[1].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.TransformEntries[1].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[1].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[1].Length.ShouldBe(0L);
                    mockAcceptFileSystemEntry.TransformEntries[2].ShouldNotBeNull();
                    mockAcceptFileSystemEntry.TransformEntries[2].Attributes.ShouldBe(System.IO.FileAttributes.Archive);
                    mockAcceptFileSystemEntry.TransformEntries[2].CreationTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[2].Directory.ShouldBe(wf.Folder + @"\subfolder");
                    mockAcceptFileSystemEntry.TransformEntries[2].FileName.ShouldBe("hello2.txt");
                    mockAcceptFileSystemEntry.TransformEntries[2].IsDirectory.ShouldBe(false);
                    mockAcceptFileSystemEntry.TransformEntries[2].IsHidden.ShouldBe(false);
                    mockAcceptFileSystemEntry.TransformEntries[2].LastAccessTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[2].LastWriteTimeUtc.ShouldBe(utcNow, tolerance);
                    mockAcceptFileSystemEntry.TransformEntries[2].Length.ShouldBe(6L);
                }


            }
        }





        private class MockAcceptFileSystemEntry : IAcceptFileSystemEntry
        {
            public List<FileSystemEntryCopy> ShouldIncludeEntries { get; private set; } = new List<FileSystemEntryCopy>();
            public List<FileSystemEntryCopy> ShouldRecurseIntoEntries { get; private set; } = new List<FileSystemEntryCopy>();
            public List<FileSystemEntryCopy> TransformEntries { get; private set; } = new List<FileSystemEntryCopy>();

            public bool ShouldIncludeEntry(in FileSystemEntry entry)
            {
                this.ShouldIncludeEntries.Add(FileSystemEntryCopy.Copy(in entry));
                return true;
            }

            public bool ShouldRecurseIntoEntry(in FileSystemEntry entry)
            {
                this.ShouldRecurseIntoEntries.Add(FileSystemEntryCopy.Copy(in entry));
                return true;
            }

            public void TransformEntry(in FileSystemEntry entry)
            {
                this.TransformEntries.Add(FileSystemEntryCopy.Copy(in entry));
            }
        }

        /// <summary>
        /// We can't store ref structs in an array (since the references would go out-of-scope)
        /// so we need to copy all the values to a less restrictive (but more memory-hungry) type
        /// </summary>
        private class FileSystemEntryCopy
        {
            public FileAttributes Attributes { get; set; }
            public DateTimeOffset CreationTimeUtc { get; set; }
            public String Directory { get; set; }
            public String FileName { get; set; }
            public bool IsDirectory { get; set; }
            public bool IsHidden { get; set; }
            public DateTimeOffset LastAccessTimeUtc { get; set; }
            public DateTimeOffset LastWriteTimeUtc { get; set; }
            public long Length { get; set; }

            public static FileSystemEntryCopy Copy(in FileSystemEntry entry)
            {
                return new FileSystemEntryCopy()
                {
                    Attributes =        entry.Attributes,
                    CreationTimeUtc =   entry.CreationTimeUtc,
                    Directory =         entry.Directory.ToString(),
                    FileName =          entry.FileName.ToString(),
                    IsDirectory =       entry.IsDirectory,
                    IsHidden =          entry.IsHidden,
                    LastAccessTimeUtc = entry.LastAccessTimeUtc,
                    LastWriteTimeUtc =  entry.LastWriteTimeUtc,
                    Length =            entry.Length,
                };
            }
        }
    }
}

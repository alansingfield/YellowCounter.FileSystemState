using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YellowCounter.FileSystemState;
using Shouldly;
using YellowCounter.FileSystemState.Options;
using NS = NSubstitute;
using YellowCounter.FileSystemState.Sizing;
using NSubstitute.Extensions;
using System.Diagnostics;

namespace PathReduxTests.Watcher
{
    [TestClass]
    public class WatcherTests
    {
        [TestMethod]
        public void FileSystemWatcherNoChange()
        {
            var dir = GetRandomDirectory();

            try
            {

                File.WriteAllText(Path.Combine(dir, "text1.txt"), "Hello");
                File.WriteAllText(Path.Combine(dir, "blah.txt"), "Hello");

                var watcher = new FileSystemState(dir, new FileSystemStateOptions().WithRecurseSubdirectories(true));
                watcher.LoadState();

                var q = watcher.GetChanges();
                q.Count.ShouldBe(0);

            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        private string GetRandomDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            return path;
        }


        [TestMethod]
        public void FileSystemWatcherBigDir()
        {
            false.ShouldBeTrue();

            string currentDir = @"C:\";

            FileSystemState watcher = new FileSystemState(currentDir, new FileSystemStateOptions().WithRecurseSubdirectories(true));
            watcher.LoadState();

            var q = watcher.GetChanges();
            q.Count.ShouldBe(0);

        }


        private class SizePolicyM : SizePolicy
        {
            public List<SizePolicyHistory> UsageCapacityHistories = new List<SizePolicyHistory>();

            public SizePolicyM(SizePolicyOptions options = null) : base(options) { }

            public override int? MustResize(int usage, int capacity)
            {
                var result = base.MustResize(usage, capacity);
                
                if(result != null)
                {
                    this.UsageCapacityHistories.Add(new SizePolicyHistory()
                    {
                        Usage = usage,
                        Capacity = capacity,
                        Result = result.Value
                    });
                }

                return result;
            }
        }

        private struct SizePolicyHistory
        {
            public int Usage { get; set; }
            public int Capacity { get; set; }
            public int Result { get; set; }
        }

        [TestMethod]
        public void FileSystemWatcherReallocationCount()
        {
            false.ShouldBeTrue();

            string currentDir = @"C:\";

            var sizePolicy1 = new SizePolicyM();
            var sizePolicy2 = new SizePolicyM();

            // Character buffer should fill without leaving gaps.
            var sizePolicy3 = new SizePolicyM(
                new SizePolicyOptions().WithFillFactor(100).WithGrowthFactor(100));

            var sizePolicy4 = new SizePolicyM();

            FileSystemState watcher = new FileSystemState(
                currentDir, 
                new FileSystemStateOptions()
                    .WithRecurseSubdirectories(true)
                    .WithFileStateReferenceSetOptions(new FileStateReferenceSetOptions()
                        .WithReferenceSetOptions(new ReferenceSetOptions()
                            .WithSizePolicy(sizePolicy1)
                        )
                    )
                    .WithPathStorageOptions(new PathStorageOptions()
                        .WithSizePolicy(sizePolicy2)
                        .WithHashedCharBufferOptions(new HashedCharBufferOptions()
                            .WithCharSizePolicy(sizePolicy3)
                            .WithHashSizePolicy(sizePolicy4)
                        )
                    )
                )
                ;

            watcher.LoadState();

            var q = watcher.GetChanges();
            q.Count.ShouldBe(0);

            foreach(var z in new [] { sizePolicy1, sizePolicy2, sizePolicy3, sizePolicy4 })
            {
                foreach(var h in z.UsageCapacityHistories)
                {
                    Debug.WriteLine($"{h.Usage, 12}|{h.Capacity, 12}|{h.Result, 12}");
                }
            }

        }
    }
}

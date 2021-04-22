using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PathReduxTests.FileSystem
{
    public class WorkingFolder : IDisposable
    {
        private bool disposedValue;
        public string Folder { get; private set; }

        public WorkingFolder()
        {
            this.Folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(this.Folder);
        }


        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    Directory.Delete(this.Folder, true);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

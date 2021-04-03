using System;
using System.Collections.Generic;
using System.Text;

namespace YellowCounter.FileSystemState.Options
{
    public static class FileStateReferenceSetOptionsExtensions
    {
        public static FileStateReferenceSetOptions WithReferenceSetOptions(
            this FileStateReferenceSetOptions fileStateReferenceSetOptions, 
            ReferenceSetOptions referenceSetOptions)
        {
            fileStateReferenceSetOptions.ReferenceSetOptions = referenceSetOptions;
            return fileStateReferenceSetOptions;
        }

    }
}

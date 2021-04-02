//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace YellowCounter.FileSystemState.Sizing
//{
//    public static class SizePolicyOptionsExtensions
//    {
//        public static SizePolicyOptions ApplyDefaults(SizePolicyOptions options)
//        {
//            if(options.FillFactor <= 0)
//                options.FillFactor = 1;

//            if(options.FillFactor > 100)
//                options.FillFactor = 100;

//            if(options.MinCapacity <= 0)
//                options.MinCapacity = 1;

//            if(options.MinFillFactor > options.FillFactor)
//                options.MinFillFactor = options.FillFactor;

//            if(options.MinFillFactor < 0)
//                options.MinFillFactor = 0;

//            return options;
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;

// https://www.youtube.com/watch?v=-H5oEgOdO6U

// https://benchmarkdotnet.org/Configs/Jobs.htm
// https://benchmarkdotnet.org/Advanced/SetupAndCleanup.htm
// https://benchmarkdotnet.org/Overview.htm
// https://benchmarkdotnet.org/Advanced/SetupAndCleanup.htm
// https://benchmarkdotnet.org/Configs/Diagnosers.htm

namespace Bnaya.Samples
{
    //[DryClrJob]
    //[ShortRunJob]
    [ClrJob]//, CoreJob]
    [MeanColumn, RankColumn]
    [RPlotExporter, RankColumn]
    //[LegacyJitX64Job] //, RyuJitX64Job]
    [MemoryDiagnoser]
    //[InliningDiagnoser]
    //[Config(typeof(Config))]
    //[Config(typeof(FastAndDirtyConfig))]
    public class ArrayBenchmark
    {
        #region Config

        private class Config : ManualConfig
        {
            public Config()
            {
                //Add(MemoryDiagnoser.Default);
                //Add(new InliningDiagnoser());
                Add(new Job(EnvMode.LegacyJitX64, EnvMode.Clr, RunMode.Short)
                {
                    Env = { Runtime = Runtime.Clr },
                    //Run = { LaunchCount = 3, WarmupCount = 5, TargetCount = 10 },
                    //Accuracy = { MaxStdErrRelative = 0.01 }
                });
            }
        }

        public class FastAndDirtyConfig : ManualConfig
        {
            public FastAndDirtyConfig()
            {
                Add(DefaultConfig.Instance); // *** add default loggers, reporters etc? ***

                Add(Job.Default
                    .WithLaunchCount(1)     // benchmark process will be launched only once
                    .WithIterationTime(TimeInterval.Microsecond * 100) // 100ms per iteration
                    .WithWarmupCount(3)     // 3 warm-up iteration
                    .WithTargetCount(3)     // 3 target iteration
                );
            }
        }

        #endregion // Config

        #region Fields

        //[Params(10_000, 100_000)]
        public int[] Data = Enumerable.Range(0, 10_000_000).Select((_,idx) => idx).ToArray();
        public Random _rnd = new Random(Guid.NewGuid().GetHashCode());

        #endregion // Fields

        #region Setup

        [GlobalSetup]
        public void Setup()
        {
        }

        #endregion // Setup

        #region CopyLoop

        [Benchmark]
        public void CopyLoop()
        {
            int[] dest = new int[Data.Length];
            for (int i = 0; i < Data.Length; i++)
            {
                dest[i] = Data[i];
            }
            int index = _rnd.Next(0, Data.Length - 1);
            if (dest[index] != Data[index])
                throw new Exception();
        }

        #endregion // CopyLoop

        #region ArrayCopy

        [Benchmark(Baseline = true)]
        public void ArrayCopy()
        {
            int[] dest = new int[Data.Length];
            Data.CopyTo(dest, 0);
            int index = _rnd.Next(0, Data.Length - 1);
            if (dest[index] != Data[index])
                throw new Exception();
        }

        #endregion // ArrayCopy

        #region BlockCopy

        [Benchmark]
        public void BlockCopy()
        {
            int[] dest = new int[Data.Length];
            Buffer.BlockCopy(Data, 0, dest, 0, Data.Length * sizeof(int));
            int index = _rnd.Next(0, Data.Length - 1);
            if (dest[index] != Data[index])
                throw new Exception();
        }

        #endregion // BlockCopy
    }
}

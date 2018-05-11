using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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
    public class SpanBenchmark
    {
        private const int BUFFER_SIZE = 1000;

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
        public int[] Data = Enumerable.Range(0, 10_000_000).ToArray();

        #endregion // Fields

        #region Setup

        [GlobalSetup]
        public void Setup()
        {
        }

        #endregion // Setup

        #region WithArray

        [Benchmark(Baseline = true)]
        public void WithArray()
        {
            var subArray = new int[BUFFER_SIZE];
            for (int i = 0; i < Data.Length - (BUFFER_SIZE + 1); i += BUFFER_SIZE)
            {
                Array.Copy(Data, i, subArray, 0, BUFFER_SIZE);
                if (subArray[subArray.Length - 1] != i + BUFFER_SIZE - 1)
                    throw new Exception(); // avoid optimization
            }
        }

        #endregion // WithArray

        // https://msdn.microsoft.com/en-us/magazine/mt814808.aspx
        // NuGet: System.Memory
        #region WithSpan

        [Benchmark]
        public void WithSpan()
        {
            var spanData = new Span<int>(Data);
            for (int i = 0; i < Data.Length - (BUFFER_SIZE + 1); i += BUFFER_SIZE)
            {
                var subSpan = spanData.Slice(i, BUFFER_SIZE);
                if (subSpan[BUFFER_SIZE - 1] != i + BUFFER_SIZE - 1)
                    throw new Exception(); 
            }
        }

        #endregion // WithSpan

        #region With...Pointer

        [Benchmark]
        public unsafe void WithPointer()
        {
            fixed (int* data = &Data[0])
            {
                for (int i = 0; i < Data.Length - (BUFFER_SIZE + 1); i += BUFFER_SIZE)
                {
                    int* sub = data + i;
                    if (sub[BUFFER_SIZE - 1] != i + BUFFER_SIZE - 1)
                        throw new Exception();
                }
            }
        }

        //[Benchmark] // Stack-overflow
        //public unsafe void WithCopyClrToStackPointer()
        //{
        //    int* data = stackalloc int[Data.Length];
        //    fixed (int* arr = &Data[0])
        //    {
        //        Buffer.MemoryCopy(arr, data, Data.Length * sizeof(int), Data.Length * sizeof(int));

        //        for (int i = 0; i < Data.Length - (BUFFER_SIZE + 1); i += BUFFER_SIZE)
        //        {
        //            int* sub = data + (i * BUFFER_SIZE);
        //            if (sub[BUFFER_SIZE - 1] != i + BUFFER_SIZE - 1)
        //                throw new Exception(); 
        //        }
        //    }
        //}

        [Benchmark]
        public unsafe void WithCopyClrToNativePointer()
        {
            IntPtr pdata = Marshal.AllocHGlobal(Data.Length * sizeof(int));
            int* data = (int*)pdata;
            try
            {
                Marshal.Copy(Data, 0, pdata, Data.Length);

                for (int i = 0; i < Data.Length - (BUFFER_SIZE + 1); i += BUFFER_SIZE)
                {
                    int* sub = data + i;
                    if (sub[BUFFER_SIZE - 1] != i + BUFFER_SIZE - 1)
                        throw new Exception();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pdata);
            }

        }

        #endregion // With...Pointer
    }
}

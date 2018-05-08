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
[assembly:SecurityTransparent] // needed by DynamicMethod

namespace Bnaya.Samples
{
    //[DryClrJob]
    [ShortRunJob]
    //[ClrJob, CoreJob]
    [MeanColumn, RankColumn]
    [RPlotExporter, RankColumn]
    //[LegacyJitX64Job] //, RyuJitX64Job]
    [MemoryDiagnoser]
    //[InliningDiagnoser]
    //[Config(typeof(Config))]
    //[Config(typeof(FastAndDirtyConfig))]
    public class DynamicCreationBenchmark
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

        private string AssemblyName;
        private string TypeName;

        //[Params(10_000, 100_000)]
        public int Iterations = 40_000;

        #endregion // Fields

        #region Setup

        [GlobalSetup]
        public void Setup()
        {
            var type = typeof(Item);
            AssemblyName = type.Assembly.FullName;
            TypeName = type.FullName;
        }

        #endregion // Setup

        #region Compiled

        [Benchmark(Baseline = true)]
        public void Compiled()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var x = new Item();
                if (x.GetType().FullName != TypeName)
                    throw new Exception(); 
            }

        }

        #endregion // Compiled

        #region ReflectionGeneric

        [Benchmark]
        public void ReflectionGeneric()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var x = Activator.CreateInstance<Item>();
                if (x.GetType().FullName != TypeName)
                    throw new Exception();
            }

        }

        #endregion // ReflectionGeneric

        #region Reflection

        [Benchmark]
        public void Reflection()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var x = (Item)Activator.CreateInstance(AssemblyName, TypeName).Unwrap();
                if (x.GetType().FullName != TypeName)
                    throw new Exception();
            }

        }

        #endregion // Reflection

        #region DynamicColdFactory

        [Benchmark] // need SecurityTransparent
        public void DynamicColdFactory()
        {
            for (int i = 0; i < Iterations; i++)
            {

                var x = GetDynamicFactory<Item>().Invoke();
                if (x.GetType().FullName != TypeName)
                    throw new Exception();
            }
        }

        #endregion // DynamicColdFactory

        #region DynamicHotFactory

        [Benchmark] // need SecurityTransparent
        public void DynamicHotFactory()
        {
            var factory = GetDynamicFactory<Item>();
            for (int i = 0; i < Iterations; i++)
            {
                var x = factory();
                if (x.GetType().FullName != TypeName)
                    throw new Exception();
            }
        }

        #endregion // DynamicHotFactory

        #region GetDynamicFactory

        private Func<T> GetDynamicFactory<T>()
        {
            var type = typeof(T);
            ConstructorInfo ctor = type.GetConstructor(Array.Empty<Type>());
            //create dynamic factory
            string methodName = type.Name + "Ctor";
            DynamicMethod dm = new DynamicMethod(methodName, type, Array.Empty<Type>(), typeof(Activator));
            ILGenerator ilgen = dm.GetILGenerator();
            ilgen.Emit(OpCodes.Newobj, ctor);
            ilgen.Emit(OpCodes.Ret);
            var factory = (Func<T>)dm.CreateDelegate(typeof(Func<T>));
            return () =>
            {
                object o = factory();
                return (T)o;
            };
        }

        #endregion // GetDynamicFactory

        public class Item { } 
    }
}

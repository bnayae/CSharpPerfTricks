using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;

namespace Bnaya.Samples
{
    static class Program
    {
        private const int LIGHT_ITERATIONS = 4;
        static void Main()
        {
            LightBenchmark();

            Console.WriteLine("Press any key for full benchmark");
            Console.ReadKey();
            RealBenchmark();

            Console.ReadLine();
        }
 
        private static void LightBenchmark()
        {
            var instance = new DynamicCreationBenchmark();
            instance.Setup();
            var sw = new Stopwatch();
            for (int i = 0; i < LIGHT_ITERATIONS; i++)
            {
                sw.Restart();
                instance.Compiled();
                sw.Stop();
                Console.WriteLine($"Compiled =              {sw.ElapsedMilliseconds.ToString("N0").PadLeft(10)}");

                sw.Restart();
                instance.Reflection();
                sw.Stop();
                Console.WriteLine($"Reflection =            {sw.ElapsedMilliseconds.ToString("N0").PadLeft(10)}");

                sw.Restart();
                instance.ReflectionGeneric();
                sw.Stop();
                Console.WriteLine($"Reflection Generic =    {sw.ElapsedMilliseconds.ToString("N0").PadLeft(10)}");

                sw.Restart();
                instance.DynamicColdFactory();
                sw.Stop();
                Console.WriteLine($"Dynamic Cold Factory =  {sw.ElapsedMilliseconds.ToString("N0").PadLeft(10)}  [SecurityTransparent]");

                sw.Restart();
                instance.DynamicHotFactory();
                sw.Stop();
                Console.WriteLine($"Dynamic Hot Factory =   {sw.ElapsedMilliseconds.ToString("N0").PadLeft(10)}  [SecurityTransparent]");

                Console.WriteLine();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine();
            }
        }
      
 
        private static void RealBenchmark()
        {
            Console.WriteLine("============ SUMMARY ==============");
            var summary = BenchmarkRunner.Run<DynamicCreationBenchmark>();
            Console.WriteLine(summary);
        }
  }
}

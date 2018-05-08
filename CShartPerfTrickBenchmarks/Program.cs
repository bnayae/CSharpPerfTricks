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
            Console.WriteLine("============ SUMMARY ==============");
            //var summary = BenchmarkRunner.Run<DynamicCreationBenchmark>();
            var summary = BenchmarkRunner.Run<ArrayBenchmark>();
            Console.WriteLine(summary);

            Console.ReadLine();
        }
       
 
  }
}

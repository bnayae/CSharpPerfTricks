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
            IBenchmark benchmark = new DynamicCreationBenchmark();
            benchmark.Run();

            Console.ReadLine();
        }
  }
}

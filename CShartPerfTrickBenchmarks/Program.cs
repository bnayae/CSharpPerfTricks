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
            //CheckNoErrors();

            //var summary = BenchmarkRunner.Run<DynamicCreationBenchmark>();
            //var summary = BenchmarkRunner.Run<ArrayBenchmark>();
            var summary = BenchmarkRunner.Run<SpanBenchmark>();
            Console.WriteLine("============ SUMMARY ==============");
            Console.WriteLine(summary);

            Console.ReadLine();
        }

        private static void CheckNoErrors()
        {
            var s = new SpanBenchmark();
            s.Setup();
            s.WithArray();
            s.WithCopyClrToNativePointer();
            //s.WithCopyClrToStackPointer();
            s.WithPointer();
            s.WithSpan();
        }

    }
}

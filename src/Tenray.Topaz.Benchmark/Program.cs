using BenchmarkDotNet.Running;
using System;
using System.Diagnostics;

#pragma warning disable CS0162 // Unreachable code detected
namespace Tenray.Topaz.Benchmark
{
    class Program
    {
        static void Main()
        {
            if (false)
            {
                RunUsingBenchmarkDotNet();
            }
            else
            {
                BenchmarkRunner.Run<Benchmark1>();
                //RunSingleJob();
            }
        }

        private static void RunSingleJob()
        {
            var sw = new Stopwatch();
            var b = new Benchmark5();
            var engine = Engines.Topaz;
            sw.Start();
            switch (engine)
            {
                case Engines.Topaz:
                    b.RunTopaz();
                    break;
                case Engines.V8Engine:
                    b.RunV8Engine();
                    break;
                case Engines.Jint:
                    b.RunJint();
                    break;
            }
            sw.Stop();
            Console.WriteLine($"{engine}: {sw.ElapsedMilliseconds} ms");
        }

        private static void RunUsingBenchmarkDotNet()
        {
            BenchmarkRunner.Run<Benchmark1>();
            BenchmarkRunner.Run<Benchmark2>();
            BenchmarkRunner.Run<Benchmark3>();
            BenchmarkRunner.Run<Benchmark4>();
            BenchmarkRunner.Run<Benchmark5>();
        }
    }
#pragma warning restore CS0162 // Unreachable code detected
}

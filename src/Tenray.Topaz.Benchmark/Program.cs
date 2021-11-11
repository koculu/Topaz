using System;
using System.Diagnostics;

namespace Tenray.Topaz.Benchmark
{
    class Program
    {
        static void Main()
        {
            var sw = new Stopwatch();
            var b = new Benchmark1();
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
    }
}

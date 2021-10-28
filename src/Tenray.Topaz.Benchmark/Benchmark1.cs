using Jint;
using System;
using System.Runtime.CompilerServices;

namespace Tenray.Topaz.Benchmark
{
    public class Benchmark1
    {
        public string Code = @"
function f1(i) {
    return i * i
}
for (var i = 0 ; i < 100000; ++i) {
    f1(i)
}
";
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private double f1(double i)
        {
            return i * i;
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public void RunNet()
        {
            for (var i = 0; i < 100000; ++i)
            {
                f1(i);
            }
        }

        public void RunTopaz()
        {
            var topazEngine = new TopazEngine();
            topazEngine.ExecuteScript(Code);
        }

        public void RunJint()
        {
            var jintEngine = new Engine();
            jintEngine.Execute(Code);
        }
    }
}

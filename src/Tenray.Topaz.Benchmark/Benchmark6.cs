using BenchmarkDotNet.Attributes;
using Jint;
using Microsoft.ClearScript.V8;
using System.Threading.Tasks;

namespace Tenray.Topaz.Benchmark
{
    public sealed class Benchmark6
    {
        public string Code = @"
function square(x) {
    return x*x;
}
";
        public int LoopLength = 100000;

        [Benchmark]
        public void RunV8Engine()
        {
            var v8Engine = new V8ScriptEngine();
            v8Engine.Execute(Code);
            var squareFunction = v8Engine.Script.square;
            Parallel.For(0, LoopLength, (x) =>
            {
                var result = v8Engine.Invoke("square", x); ;
            });
        }

        [Benchmark]
        public void RunTopaz()
        {
            var topazEngine = new TopazEngine(new TopazEngineSetup
            {
                IsThreadSafe = false
            });
            topazEngine.ExecuteScript(Code);
            var squareFunction = topazEngine.GetValue("square");
            Parallel.For(0, LoopLength, (x) =>
            {
                var result = topazEngine.InvokeFunction(squareFunction, default, x);
            });
        }

        [Benchmark]
        public void RunJint()
        {
            var jintEngine = new Engine();
            jintEngine.Execute(Code);
            var squareFunction = jintEngine.GetValue("square");
            var syncObject = new object();
            Parallel.For(0, LoopLength, (x) =>
            {
                lock (syncObject)
                {
                    var result = jintEngine.Invoke(squareFunction, null, x);
                }
            });
        }

    }
}

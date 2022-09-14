using BenchmarkDotNet.Attributes;
using Jint;
using Microsoft.ClearScript.V8;
using System.Collections.Generic;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Benchmark
{
    public sealed class Benchmark3
    {
        public string Code = @"
for (var i = 0.0 ; i < 1000000; ++i) {
    model.Value++;
}
";
        public sealed class Model
        {
            public int Value;
        }

        [Benchmark]
        public void RunTopaz()
        {
            var topazEngine = new TopazEngine(new TopazEngineSetup
            {
                IsThreadSafe = false
            });
            var model = new Dictionary<string,int>();
            model.Add("Value", 0);
            topazEngine.SetValue("model", model);
            topazEngine.ExecuteScript(Code);
        }

        // [Benchmark]
        public void RunV8Engine()
        {
            var v8Engine = new V8ScriptEngine();
            var model = new Dictionary<string, int>();
            model.Add("Value", 0);
            v8Engine.AddHostObject("model", model);
            v8Engine.Execute(Code);
        }

        [Benchmark]
        public void RunJint()
        {
            var jintEngine = new Engine();
            var model = new Dictionary<string, int>();
            model.Add("Value", 0);
            jintEngine.SetValue("model", model);
            jintEngine.Execute(Code);
        }
    }
}

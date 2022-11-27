using BenchmarkDotNet.Attributes;
using Jint;
using Microsoft.ClearScript.V8;
using Tenray.Topaz.API;
using Tenray.Topaz.Interop;

namespace Tenray.Topaz.Benchmark;

public class Benchmark2
{
    public string Code = @"
for (var i = 0.0 ; i < 1000000; ++i) {
    model.Value++;
}
";

    public class Model
    {
        public int Value;
    }

    [Benchmark]
    public void RunTopaz()
    {
        var topazEngine = new TopazEngine(new TopazEngineSetup
        {
            IsThreadSafe = true,
        });
        var model = new Model();
        topazEngine.SetValue("model", model);
        topazEngine.ExecuteScript(Code);
    }

    // [Benchmark]
    public void RunV8Engine()
    {
        var v8Engine = new V8ScriptEngine();
        var model = new Model();
        v8Engine.AddHostObject("model", model);
        v8Engine.Execute(Code);
    }

    [Benchmark]
    public void RunJint()
    {
        var jintEngine = new Engine();
        var model = new Model();
        jintEngine.SetValue("model", model);
        jintEngine.Execute(Code);
    }
}

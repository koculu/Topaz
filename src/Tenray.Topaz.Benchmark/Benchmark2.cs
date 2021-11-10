﻿using Jint;
using Microsoft.ClearScript.V8;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Benchmark
{
    public class Benchmark2
    {
        public string Code = @"
for (var i = 0.0 ; i < 10000000; ++i) {
    model.Value++;
}
";
        public class Model
        {
            public int Value;
        }

        public void RunTopaz()
        {
            var topazEngine = new TopazEngine(true);
            var model = new Model();
            topazEngine.SetValue("model", model);
            topazEngine.ExecuteScript(Code);
        }

        public void RunV8Engine()
        {
            var v8Engine = new V8ScriptEngine();
            var model = new Model();
            v8Engine.AddHostObject("model", model);
            v8Engine.Execute(Code);
        }

        public void RunJint()
        {
            var jintEngine = new Engine();
            var model = new Model();
            jintEngine.SetValue("model", model);
            jintEngine.Execute(Code);
        }
    }
}

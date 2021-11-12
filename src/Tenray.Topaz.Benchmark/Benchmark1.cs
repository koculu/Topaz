using Jint;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Tenray.Topaz.Benchmark
{
    public class Benchmark1
    {
        public string CodeParallel = @"
Parallel.For(0, 10000000 , (i) => i + i)
";
        public string Code = @"
f1 = (i) => i * i

for (var i = 0.0 ; i < 10000000; ++i) {
    f1(i)
}
";
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private double f1(double i)
        {
            return i * i;
        }

        internal void RunV8Engine()
        {
            var v8Engine = new V8ScriptEngine();
            v8Engine.Execute(Code);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public void RunNet()
        {
            for (var i = 0; i < 100000; ++i)
            {
                f1(i);
            }
        }

        public bool RunParallelTopaz()
        {
            var topazEngine = new TopazEngine(new TopazEngineSetup
            {
                IsThreadSafe = true
            });
            topazEngine.SetValue("double", new Func<object, double>(x => Convert.ToDouble(x)));
            topazEngine.AddType(typeof(Parallel), "Parallel");
            topazEngine.ExecuteScript(CodeParallel);
            return true;
        }

        public bool RunTopaz()
        {
            var topazEngine = new TopazEngine(new TopazEngineSetup
            {
                IsThreadSafe = false
            });
            topazEngine.ExecuteScript(Code);
            return true;
        }

        public void RunJint()
        {
            var jintEngine = new Engine();
            jintEngine.Execute(Code);
        }

        public void Playground()
        {
            var topazEngine = new TopazEngine();
            topazEngine.SetValue("int", typeof(int));
            topazEngine.SetValue("string", typeof(string));
            topazEngine.SetValue("forEach", 
                new Action<int, int, Action<int>>(
                    (start, end, y) =>
                    {
                        for (var i = start; i < end; ++i)
                        {
                            y(i);
                        }
                    }));
            topazEngine.AddType<HttpClient>("HttpClient");
            topazEngine.AddType(typeof(Console), "Console");
            topazEngine.AddType(typeof(Enumerable), "Enumerable");
            topazEngine.AddType(typeof(Parallel), "Parallel");
            topazEngine.AddType(typeof(Action), "Action");
            topazEngine.AddType(typeof(Dictionary<,>), "GenericDictionary");
            topazEngine.AddType(typeof(Action<,>), "Action2d");
            topazEngine.AddType(typeof(Func<,>), "Func2d");
            /*topazEngine.ExecuteScript(@"
var dic = new GenericDictionary(string, int)
dic.Add('hello', 1)
dic.Add('dummy', 0)
dic.Add('world', 2)
dic.Remove('dummy')
Console.WriteLine(`Final value: {dic['hello']}`);
");*/
            topazEngine.ExecuteScript(@"

function f3(i) {
g = x * x
return g
}

var func = new Func2d(int,int,f3)
Console.WriteLine('func:' + func(9))

function f2(s,i) {
Console.WriteLine('f2:' + s + i);
return s + i
}

//forEach(0,100000, f3);

var g = 8
function f1(x, y) {
return x * x
//Console.WriteLine('f1:' + x + y.IsExceptional)
}
var a = new Action2d(string, int, f2)
a(3,9)

Parallel.For(0, 1000000 , f1)
");
        }

    }
}

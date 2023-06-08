using BenchmarkDotNet.Attributes;
using Jint;
using Jint.Runtime.Interop;
using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Tenray.Topaz.Benchmark;

public class Benchmark1
{
    public string CodeParallel = @"
Parallel.For(0, 1000000 , (i) => i + i)
";
    public string Code = @"
f1 = (i) => i * i

for (var i = 0.0 ; i < 1000000; ++i) {
    f1(i)
}
"; 
    public string CodeHostLoop = @"
Host.For(0, 1000000 , (i) => i + i)
";
    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    private double f1(double i)
    {
        return i * i;
    }

    [Benchmark]
    public void RunV8Engine()
    {
        var v8Engine = new V8ScriptEngine();
        v8Engine.Execute(Code);
    }

    public static class Host
    {
        public static void For(int fromInclusive, int toExclusive, Action<int> action)
        {
            for(var i = fromInclusive; i < toExclusive; ++i)
                action(i);
        }
    }
    public class HostObj
    {
        public void For(
            double fromInclusive, 
            double toExclusive,
            dynamic action)
        {
            for (var i = fromInclusive; i < toExclusive; ++i)
                action(i);
        }
    }

    [Benchmark]
    public void RunV8EngineHostLoop()
    {
        var v8Engine = new V8ScriptEngine();
        v8Engine.AddHostObject("Host", new HostObj());            
        v8Engine.Execute(CodeHostLoop);
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public void RunNet()
    {
        for (var i = 0; i < 100000; ++i)
        {
            f1(i);
        }
    }

    [Benchmark]
    public void RunParallelTopaz()
    {
        var topazEngine = new TopazEngine(new TopazEngineSetup
        {
            IsThreadSafe = true
        });
        topazEngine.AddType(typeof(Parallel), "Parallel");
        topazEngine.ExecuteScript(CodeParallel);
    }

    [Benchmark]
    public void RunTopazHostLoop()
    {
        var topazEngine = new TopazEngine(new TopazEngineSetup
        {
            IsThreadSafe = true
        });
        topazEngine.AddType(typeof(Host), "Host");
        topazEngine.ExecuteScript(CodeHostLoop);
    }

    [Benchmark]
    public void RunTopaz()
    {
        var topazEngine = new TopazEngine(new TopazEngineSetup
        {
            IsThreadSafe = false
        });
        topazEngine.ExecuteScript(Code);
    }

    [Benchmark]
    public void RunJint()
    {
        var jintEngine = new Engine();
        jintEngine.Execute(Code);
    }

    [Benchmark]
    public void RunJintHostLoop()
    {
        var jintEngine = new Engine();
        jintEngine.SetValue("Host", 
            TypeReference.CreateTypeReference(jintEngine, typeof(Host)));
        jintEngine.Execute(CodeHostLoop);
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

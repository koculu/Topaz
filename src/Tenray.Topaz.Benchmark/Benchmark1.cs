using Jint;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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

        public void Playground()
        {
            var topazEngine = new TopazEngine();
            topazEngine.AddType<Uri>("Uri");
            topazEngine.AddType<HttpClient>("HttpClient");
            topazEngine.AddType(typeof(Console), "Console");
            var task = topazEngine.ExecuteScriptAsync(@"
async function httpGet(url) {
    try {
        var httpClient = new HttpClient()
        var response = await httpClient.GetAsync(url)
        return await response.Content.ReadAsStringAsync()
    }
    catch (err) {
        Console.WriteLine('Caught Error:\n' + err)
    }
    finally {
        httpClient.Dispose();
    }
}
const html = await httpGet('http://example.com')
Console.WriteLine(html);
");
            task.Wait();
        }

    }
}

using NUnit.Framework;
using System;
using System.Net.Http;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class AsyncFunctionTests
    {
        [Test]
        public void HttpGetAsync()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.AddType<HttpClient>("HttpClient");
            engine.AddType(typeof(Console), "Console");
            engine.SetValue("model", model);
            engine.AddType<Uri>("Uri");
            var task = engine.ExecuteScriptAsync(@"
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
        httpClient.Dispose()
    }
}
const html = model.html = await httpGet('http://example.com')
Console.WriteLine(html);
");
            task.Wait();
            Assert.IsNotNull(model.html);
            Assert.IsTrue(model.html.GetType() == typeof(string));
            Assert.IsTrue(model.html.StartsWith("<!doctype html>"));
        }
    }
}
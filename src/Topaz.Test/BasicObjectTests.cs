using NUnit.Framework;
using System;
using System.Text.Json;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test;

public sealed class BasicObjectTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void TestObject1(bool useThreadSafeJsObjects)
    {
        var engine = new TopazEngine();
        engine.Options.NoUndefined = true;
        engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
        engine.AddType<DateTime>("DateTime");
        dynamic model = new JsObject();
        engine.SetValue("model", model);
        engine.ExecuteScript(@"
var a = {
    nested: {
        n1: 1,
        n2: 2,
        n3 : 3
    }
    a1: 1
    'a2': 2    
}
a.a3 = 3
a[new DateTime()] = 4
a[null] = 5
model.js = a
model.p1 = a.p1
");
        var js = model.js;
        var json = JsonSerializer.Serialize((object)js);
        Console.WriteLine(json);
        dynamic deserialized;
        if (useThreadSafeJsObjects)
        {
            deserialized = JsonSerializer.Deserialize<ConcurrentJsObject>(json);
            var keys = deserialized.Keys;
            var values = deserialized.Values;
            Assert.AreEqual(6, keys.Count);
            Assert.AreEqual(6, values.Count);
            Assert.AreEqual(js, deserialized);
        }
        else
        {
            deserialized = JsonSerializer.Deserialize<JsObject>(json);
            var keys = deserialized.Keys;
            var values = deserialized.Values;
            Assert.AreEqual(6, keys.Count);
            Assert.AreEqual(6, values.Count);
            Assert.AreEqual(js, deserialized);
        }

        Assert.AreEqual(1, js.a1);
        Assert.AreEqual(2, js.a2);
        Assert.AreEqual(3, js.a3);
        Assert.AreEqual(4, js[new DateTime()]);
        Assert.AreEqual(5, js[null]);
        Assert.IsNull(model.p1);
    }

    [TestCase]
    public void TestInQuery()
    {
        var engine = new TopazEngine();
        engine.ExecuteScript("let obj = { a: { b: '123' }}");
        Assert.That(engine.ExecuteExpression("'b' in obj.a"), Is.True);
        Assert.That(engine.ExecuteExpression("obj.a.Contains('b')"), Is.True);
        Assert.That(engine.ExecuteExpression("obj.a.hasOwn('b')"), Is.True);
        Assert.That(engine.ExecuteExpression("obj.a.hasOwnProperty('b')"), Is.True);
        Assert.That(engine.ExecuteExpression("'c' in obj.a"), Is.False);
        Assert.That(engine.ExecuteExpression("obj.a.Contains('c')"), Is.False);
        Assert.That(engine.ExecuteExpression("obj.a.hasOwn('c')"), Is.False);
        Assert.That(engine.ExecuteExpression("obj.a.hasOwnProperty('c')"), Is.False);
    }
}
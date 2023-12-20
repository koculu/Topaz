using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test;

public sealed class AwaitTests
{
    public async ValueTask<int> GenericValueTask()
    {
        await Task.Delay(1);
        return 33;
    }

    public async ValueTask SimpleValueTask()
    {
        await Task.Delay(1);
        return;
    }

    public async Task<int> GenericTask()
    {
        await Task.Delay(1);
        return 33;
    }

    public async Task SimpleTask()
    {
        await Task.Delay(1);
        return;
    }

    [Test]
    public void GenericValueTaskAwait()
    {
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("test", this);
        engine.SetValue("model", model);
        engine.ExecuteScriptAsync(@"
model.result1 = await test.GenericValueTask();
").Wait();
        Assert.That(model.result1, Is.EqualTo(33));
        engine.ExecuteScript(@"
model.result2 = await test.GenericValueTask();
");
        Assert.That(model.result2, Is.EqualTo(33));
    }

    [Test]
    public void SimpleValueTaskAwait()
    {
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("test", this);
        engine.SetValue("model", model);
        engine.ExecuteScriptAsync(@"
model.result1 = await test.SimpleValueTask();
").Wait();
        Assert.That(model.result1, Is.EqualTo(null));
        engine.ExecuteScript(@"
model.result2 = await test.SimpleValueTask();
");
        Assert.That(model.result2, Is.EqualTo(null));
    }

    [Test]
    public void GenericTaskAwait()
    {
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("test", this);
        engine.SetValue("model", model);
        engine.ExecuteScriptAsync(@"
model.result1 = await test.GenericTask();
").Wait();
        Assert.That(model.result1, Is.EqualTo(33));
        engine.ExecuteScript(@"
model.result2 = await test.GenericTask();
");
        Assert.That(model.result2, Is.EqualTo(33));
    }

    [Test]
    public void SimpleTaskAwait()
    {
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("test", this);
        engine.SetValue("model", model);
        engine.ExecuteScriptAsync(@"
model.result1 = await test.SimpleTask();
").Wait();
        Assert.That(model.result1, Is.EqualTo(null));
        engine.ExecuteScript(@"
model.result2 = await test.SimpleTask();
");
        Assert.That(model.result2, Is.EqualTo(null));
    }
}
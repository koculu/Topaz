using NUnit.Framework;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test;

public sealed class GlobalThisTests
{
    [Test]
    public void TestGlobalThis1()
    {
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("globalThis", new GlobalThis(engine.GlobalScope));
        engine.SetValue("model", model);
        engine.SetValue("JSON", new JSONObject());
        engine.ExecuteScript(@"
model.a = globalThis.JSON
model.b = globalThis.model
var x = 3
model.c = globalThis.x
");
        Assert.AreEqual("3", ((JSONObject)model.a).stringify(3));
        Assert.AreEqual(model, model.b);
        Assert.AreEqual(3, model.c);
    }
}
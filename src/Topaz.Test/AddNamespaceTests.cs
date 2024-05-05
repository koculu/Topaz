using NUnit.Framework;
using System.Collections;
using System.Text.RegularExpressions;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test;

public sealed class AddNamespaceTests
{
    [Test]
    public void TestAddNamespace1()
    {
        var engine = new TopazEngine();
        engine.AddNamespace("System", null, true);
        dynamic model = new JsObject();
        engine.SetValue("model", model);
        engine.ExecuteScript(@"
        let sb = new System.Text.StringBuilder()
        sb.Append('1234')
        model.data = sb.ToString()
");
        Assert.That(model.data, Is.EqualTo("1234"));
    }

    [Test]
    public void TestAddNamespace2()
    {
        var engine = new TopazEngine();
        engine.AddNamespace("System.Text", null, true);
        dynamic model = new JsObject();
        engine.SetValue("model", model);

        engine.ExecuteScript(@"
        let sb = new System.Text.StringBuilder()
        sb.Append('1234')
        model.data = sb.ToString()
        model.reg = new System.Text.RegularExpressions.Regex('\w');
");
        Assert.That(model.data, Is.EqualTo("1234"));
        Assert.That(model.reg, Is.InstanceOf<Regex>());
    }

    [Test]
    public void TestAddNamespace3()
    {
        var engine = new TopazEngine();
        engine.AddNamespace("System.Text", null, false);
        dynamic model = new JsObject();
        engine.SetValue("model", model);

        engine.ExecuteScript(@"
        let sb = new System.Text.StringBuilder()
        sb.Append('1234')
        model.data = sb.ToString()
        model.reg = System.Text.RegularExpressions.Regex;
");
        Assert.That(model.data, Is.EqualTo("1234"));
        Assert.That(model.reg, Is.Null);
    }

    [Test]
    public void TestAddNamespace4()
    {
        var engine = new TopazEngine();
        engine.AddNamespace("System.Text", null, true);
        dynamic model = new JsObject();
        engine.SetValue("model", model);

        engine.ExecuteScript(@"
        let sb = new System.Text.StringBuilder()
        sb.Append('1234')
        model.data = sb.ToString()
        model.appDomain = System.AppDomain;
");
        Assert.That(model.data, Is.EqualTo("1234"));
        Assert.That(model.appDomain, Is.Null);
    }
}
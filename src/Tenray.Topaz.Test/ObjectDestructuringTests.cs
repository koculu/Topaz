using NUnit.Framework;
using System;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test;

public sealed class ObjectDestructuringTests
{
    [Test]
    public void DefineObjectWithVariables()
    {
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("model", model);
        engine.ExecuteScript(@"
var a = 5, g = 9
var rest = [1,2,3], kest = [1,2,4]
var b = {
    a,
    ...rest,
    ...kest,
    g
}
model.b = b
");
        Assert.AreEqual(5, model.b.a);
        Assert.AreEqual(1, model.b["0"]);
        Assert.AreEqual(2, model.b["1"]);
        Assert.AreEqual(4, model.b["2"]);
        Assert.AreEqual(9, model.b.g);
    }

    [Test]
    public void DefineVariablesWithObjectAssignmentPattern()
    {
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("model", model);
        engine.ExecuteScript(@"
({ a, b = 3 } = { a: 1 })
model.a = a
model.b = b;
({nan, y = 33} = NaN)
model.nan = nan
model.y = y;
({ a, b = 3, ...c } = { a: 1, c: [11,22,33] })
model.c = c
");
        Assert.AreEqual(1, model.a);
        Assert.AreEqual(3, model.b);
        Assert.AreEqual(null, model.nan);
        Assert.AreEqual(33, model.y);
        Assert.AreEqual(11, model.c[0]);
        Assert.AreEqual(22, model.c[1]);
        Assert.AreEqual(33, model.c[2]);
    }

    [Test]
    public void DefineVariablesWithObjectDestructuring()
    {
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("model", model);
        engine.ExecuteScript(@"
var { a, b = 3 } = { a: 1 }
model.a = a
model.b = b;
var {nan, y = 33} = NaN
model.nan = nan
model.y = y;
var { a, b = 3, ...c } = { a: 1, c: [11,22,33] }
model.c = c
var { foo, bar } = { foo: 'lorem', bar: 'ipsum' };
model.foo = foo
model.bar = bar
");
        Assert.AreEqual(1, model.a);
        Assert.AreEqual(3, model.b);
        Assert.AreEqual(null, model.nan);
        Assert.AreEqual(33, model.y);
        Assert.AreEqual(11, model.c[0]);
        Assert.AreEqual(22, model.c[1]);
        Assert.AreEqual(33, model.c[2]);
        Assert.AreEqual("lorem", model.foo);
        Assert.AreEqual("ipsum", model.bar);
    }

    [Test]
    public void NestedObjectDestructuring()
    {
        var engine = new TopazEngine();
        engine.Options.NoUndefined = false;
        dynamic model = new JsObject();
        engine.SetValue("model", model);
        engine.ExecuteScript(@"
var complicatedObj = {
  arrayProp: [
    'Zapp',
    { 
        second: 'Brannigan',
        third: {
            cool: 'deeper'
        }
    }
  ]
};
var { arrayProp: [first, { second, third: { cool } }] } = complicatedObj;
model.first = first
model.second = second
model.arrayProp = arrayProp
model.cool = cool
");
        Assert.AreEqual("Zapp", model.first);
        Assert.AreEqual("Brannigan", model.second);
        Assert.AreEqual(Undefined.Value, model.arrayProp);
        Assert.AreEqual("deeper", model.cool);
    }

    [Test]
    public void TestMethodArgumentDestructuring()
    {
        var js = @"
let getFullName = ({firstName, lastName}) => `${firstName} ${lastName}`;

let person = {
    firstName: 'John',
    lastName: 'Bruno'
};

model.fullname = getFullName(person);
";
        var engine = new TopazEngine();
        dynamic model = new JsObject();
        engine.SetValue("model", model);
        engine.ExecuteScript(js);

        Assert.AreEqual("John Bruno", model.fullname);
    }

}
using NUnit.Framework;
using Tenray.Topaz.API;
using Tenray.Topaz.Options;

namespace Tenray.Topaz.Test
{
    public class FunctionTests
    {
        [Test]
        public void VariableCapturing()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var funcs = [0, 0, 0]
let i = 3
for (let i = 0; i < 3; ++i) {
    while(true) {
        funcs[i] = function() {
            model[i] = i
        }
        break
    }
}
for (let j = 0; j < 3; j++) {
  funcs[j] ()
}
");
            Assert.AreEqual(0, model["0"]);
            Assert.AreEqual(1, model["1"]);
            Assert.AreEqual(2, model["2"]);
        }

        [Test]
        public void VariableCapturingConst()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var funcs = [0, 0, 0]
let i = 3
for (const i in funcs) {
    while(true) {
        funcs[i] = function() {
            model[i] = i
        }
        break
    }
}
for (let j = 0; j < 3; j++) {
  funcs[j] ()
}
");
            Assert.AreEqual(0, model["0"]);
            Assert.AreEqual(1, model["1"]);
            Assert.AreEqual(2, model["2"]);
        }

        [Test]
        public void DefaultValueAssignment()
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = false;
            engine.Options.AllowUndefinedReferenceAccess = true;
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var model = model ?? {}
awesomeFunction = function (url='example.com',
{
  opt1 = true,
  opt2 = 'post',
  opt3 = 123
})
{
    model.url = url
    model.opt1 = opt1
    model.opt2 = opt2
    model.opt3 = opt3
}
awesomeFunction('test.com', {opt3: 'Charming'});
model
");
            Assert.AreEqual("test.com", model.url);
            Assert.AreEqual(true, model.opt1);
            Assert.AreEqual("post", model.opt2);
            Assert.AreEqual("Charming", model.opt3);
            engine.ExecuteExpression("awesomeFunction('beta.com')");
            Assert.AreEqual("beta.com", model.url);
            Assert.AreEqual(Undefined.Value, model.opt1);
            Assert.AreEqual(Undefined.Value, model.opt2);
            Assert.AreEqual(Undefined.Value, model.opt3);
        }

        [Test]
        public void DoubleDefaultValueAssignment()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var model = model ?? {}
awesomeFunction = function (url='example.com',
{
  opt1 = true,
  opt2 = 'post',
  opt3 = 123
} = { opt1: 'xyz' })
{
    model.url = url
    model.opt1 = opt1
    model.opt2 = opt2
    model.opt3 = opt3
}
awesomeFunction('test.com',{opt3: 'Charming'});
model
");
            Assert.AreEqual("test.com", model.url);
            Assert.AreEqual(true, model.opt1);
            Assert.AreEqual("post", model.opt2);
            Assert.AreEqual("Charming", model.opt3);
            engine.ExecuteExpression("awesomeFunction('beta.com')");
            Assert.AreEqual("beta.com", model.url);
            Assert.AreEqual("xyz", model.opt1);
            Assert.AreEqual("post", model.opt2);
            Assert.AreEqual(123, model.opt3);
            engine.ExecuteExpression("awesomeFunction()");
            Assert.AreEqual("example.com", model.url);
            Assert.AreEqual("xyz", model.opt1);
            Assert.AreEqual("post", model.opt2);
            Assert.AreEqual(123, model.opt3);
            engine.ExecuteExpression("awesomeFunction(5, { opt1: 333 })");
            Assert.AreEqual(5, model.url);
            Assert.AreEqual(333, model.opt1);
            Assert.AreEqual("post", model.opt2);
            Assert.AreEqual(123, model.opt3);
        }

        [Test]
        public void ClosureVariableSharing()
        {
            var engine = new TopazEngine();
            engine.Options.AssignmentWithoutDefinitionBehavior
                = AssignmentWithoutDefinitionBehavior.DefineAsVarInGlobalScope;
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var model = model ?? {}
var a = 1
let b = 2
function f1(x)
{
    a = 2
    model.capturedB = b
    b = b + x
    model.modifiedCapturedB = b
    c = 8
    model.c = c
}
f1(1)
model.a = a
model.b = b
model.outerC = c
");
            Assert.AreEqual(2, model.a);
            Assert.AreEqual(2, model.capturedB);
            Assert.AreEqual(3, model.b);
            Assert.AreEqual(3, model.modifiedCapturedB);
            Assert.AreEqual(8, model.c);
            Assert.AreEqual(8, model.outerC);
        }

        [Test]
        public void ClosureVariableSharing2()
        {
            var engine = new TopazEngine();
            engine.Options.AssignmentWithoutDefinitionBehavior
                = AssignmentWithoutDefinitionBehavior.DefineAsVarInFirstChildOfGlobalScope;
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var model = model ?? {}
var a = 1
let b = 2
function f1(x)
{
    a = 2
    model.capturedB = b
    b = b + x
    model.modifiedCapturedB = b
    c = 8
    model.c = c
    function f2() {
        y = 9
        model.y = y
    }
    f2()
    model.outerY = y
}
f1(1)
model.a = a
model.b = b
model.outerC = c
model.globalY = y
");
            Assert.AreEqual(2, model.a);
            Assert.AreEqual(2, model.capturedB);
            Assert.AreEqual(3, model.b);
            Assert.AreEqual(3, model.modifiedCapturedB);
            Assert.AreEqual(8, model.c);
            Assert.AreEqual(null, model.outerC);
            Assert.AreEqual(9, model.y);
            Assert.AreEqual(9, model.outerY);
            Assert.AreEqual(null, model.globalY);
        }

        [Test]
        public void ClosureVariableSharing3()
        {
            var engine = new TopazEngine();
            engine.Options.AssignmentWithoutDefinitionBehavior
                = AssignmentWithoutDefinitionBehavior.DefineAsVarInFirstChildOfGlobalScope;
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var model = model ?? {}
var a = 1
function f1(x)
{
    function f2(y) {
        a = x + y
        model.a = a
        b = x * y
        return a + b
    }
    model.b = b
    return f2
}
model.p = f1(1)(3)
model.q = f1(2)(4)
model.a = a
");
            Assert.AreEqual(6, model.a);
            Assert.AreEqual(null, model.b);
            Assert.AreEqual(7, model.p);
            Assert.AreEqual(14, model.q);
        }
    }
}
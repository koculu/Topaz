using NUnit.Framework;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class ArrayPatternTests
    {
        [Test]
        public void MissingElements()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var [,,third] = ['foo', 'bar', 'baz'];
model.baz = third
");
            Assert.AreSame(Undefined.Value, model["foo"]);
            Assert.AreEqual("baz", model.baz);
        }

        [TestCase("var")]
        [TestCase("let")]
        [TestCase("const")]
        [TestCase("")]
        public void MultipleDeclarationWithRest(string variableKind)
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@$"
{variableKind} [a, b, ...rest] = [10, 20, 30, 40, 50], d = 2;
model.a = a
model.b = b
model.rest = rest
model.d = d
");
            Assert.AreEqual(10, model.a);
            Assert.AreEqual(20, model.b);
            Assert.AreEqual(30, model.rest[0]);
            Assert.AreEqual(40, model.rest[1]);
            Assert.AreEqual(50, model.rest[2]);
            Assert.AreEqual(2, model.d);
        }

        [Test]
        public void DefaultValueAssignmentPattern()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var f = (x=1) => x * x
model.a = f()
model.b = f(2)
model.c = f(3,4)
model.d = f(null,5)
");
            Assert.AreEqual(1, model.a);
            Assert.AreEqual(4, model.b);
            Assert.AreEqual(9, model.c);
            Assert.AreEqual(0, model.d);
        }

        [TestCase("var")]
        [TestCase("let")]
        [TestCase("const")]
        [TestCase("")]
        public void NestedAssignment(string variableKind)
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@$"
{variableKind} [a, [[b], c]] = [1, [[2], 3]]
model.a = a
model.b = b
model.c = c
");
            Assert.AreEqual(1, model.a);
            Assert.AreEqual(2, model.b);
            Assert.AreEqual(3, model.c);
        }

        [Test]
        public void ArrayInitializationWithVariables()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = 1, b = 2, c = [3, 4]
var d = [a, b, ...c, 5]
model.d = d
");
            Assert.AreEqual(1, model.d[0]);
            Assert.AreEqual(2, model.d[1]);
            Assert.AreEqual(3, model.d[2]);
            Assert.AreEqual(4, model.d[3]);
            Assert.AreEqual(5, model.d[4]);
        }

        [Test]
        public void ArrayVariableUpdateVariableEdgeCase()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = 1, b = 2, c = 3
d = [a, b, c]
b = 3
model.d = d
");
            Assert.AreEqual(2, model.d[1]);
        }

        [Test]
        public void ObjectVariableUpdateVariableEdgeCase()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = 1, b = 2, c = 3
d = {a, b, c}
b = 3
model.d = d
");
            Assert.AreEqual(2, model.d.b);
        }

        [Test]
        public void ArrayExpressionWithRest()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("JSON", new JSONObject());
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = [1,2,3]
var b = [5,...a,6,7]
model.b = JSON.stringify(b)
");
            Assert.AreEqual("[5,1,2,3,6,7]", model.b);
        }
    }
}
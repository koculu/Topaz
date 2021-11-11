using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class LocalVariableCacheTests
    {
        [Test]
        public void TestIdentifierSharing()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.AddType(typeof(Action), "Action");
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
function f1(p1) {
    model.a = p1
}
function f2(p1) {
    model.b = p1
}
f1(3)
f2(5)
");
            Assert.AreEqual(3, model.a);
            Assert.AreEqual(5, model.b);
        }

        [Test]
        public void TestIdentifierSharingParallel()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.AddType(typeof(Action), "Action");
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
function f1(p1) {
    model.a = p1
}
function f2(p1) {
    model.b = p1
}
f1(3)
f2(5)
");
            var random = new Random();
            Parallel.For(0, 10000, (x) =>
            {
                if (random.Next(0, 1) == 1)
                    engine.InvokeFunction("f1", default, 3);
                else
                    engine.InvokeFunction("f2", default, 5);
                Assert.AreEqual(3, model.a);
                Assert.AreEqual(5, model.b);
            });
            Assert.AreEqual(3, model.a);
            Assert.AreEqual(5, model.b);
        }

        public class TestModel
        {
            public int Value = 0;
        }

        [Test]
        public void TestVariableAdjustmentInScope()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.AddType(typeof(Action), "Action");
            engine.SetValue("model", model);
            engine.SetValue("a1", new TestModel());
            engine.SetValue("a2", new TestModel());
            engine.ExecuteScript(@"
for (var test of [a1,a2])
{
    test.Value++;
    test = a2;
    test.Value++;
}
model.a = a1.Value
model.b = a2.Value
");
            Assert.AreEqual(1, model.a);
            Assert.AreEqual(3, model.b);
        }

        [Test]
        public void TestLetVariableAdjustmentInScope()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.AddType(typeof(Action), "Action");
            engine.SetValue("model", model);
            engine.SetValue("a1", new TestModel());
            engine.SetValue("a2", new TestModel());
            engine.ExecuteScript(@"
for (let test of [a1,a2])
{
    test.Value++;
    test = a2;
    test.Value++;
}
model.a = a1.Value
model.b = a2.Value
");
            Assert.AreEqual(1, model.a);
            Assert.AreEqual(3, model.b);
        }

        [Test]
        public void TestNestedFunctionScopeVariables()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.AddType(typeof(Action), "Action");
            engine.SetValue("model", model);
            engine.SetValue("a1", new TestModel());
            engine.SetValue("a2", new TestModel());
            engine.ExecuteScript(@"
var list = []
var a = 5
function f1() {
 function f2() {
   list.push(a)
 }
 f2()
 var  a = 8
 f2()
}
f1()
a=4;
f1()
model.list = list
");
            Assert.AreEqual(4, model.list.length);
            // v8 list[0] is undefined.
            // to achieve same behavior we have to process variable declarations first.
            // for performance reasons we don't do that.
            // TODO: if same bahaviour is required, add an optional
            // pre-process and mark all variables once before script execution.
            Assert.AreEqual(5, model.list[0]); 
            Assert.AreEqual(8, model.list[1]);
            Assert.AreEqual(4, model.list[2]);
            Assert.AreEqual(8, model.list[3]);
        }
    }
}
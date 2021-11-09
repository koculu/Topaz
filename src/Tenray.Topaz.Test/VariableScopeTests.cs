using NUnit.Framework;
using Tenray.Topaz.API;
using Tenray.Topaz.Options;

namespace Tenray.Topaz.Test
{
    public class VariableScopeTests
    {
        [Test]
        public void VarScopeLoop()
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.VarScopeBehavior = VarScopeBehavior.FunctionScope;
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
function equals(a,b) { if (a !== b) throw `${a ?? 'null'} != ${b ?? 'null'}` }
function f1() { { { { d = 9; equals(d,9) } equals(d,9) } } model.d = d } f1()
function f1() { { { { var g = 9; equals(g,9) } equals(g,9) } } model.g = g } f1()
function f2() { { { { let m = 9; equals(m,9) } equals(m,null) } } model.m = m } f2()
function f3() { { { { const u = 9; equals(u,9) } equals(u,null) } } model.u = u } f3()
");
            Assert.AreEqual(9, model.d);
            Assert.AreEqual(9, model.g);
            Assert.AreEqual(null, model.m);
            Assert.AreEqual(null, model.u);
        }

        [Test]
        public void ClosureTest()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
function makeFunc() {
    var name = 'Mozilla';
    function displayName()
    {
        model.name = name;
    }
    return displayName;
}   
var myFunc = makeFunc();
myFunc();

function makeAdder(x) {
  return function(y) {
    return x + y;
  };
}

var add5 = makeAdder(5)
var add10 = makeAdder(10)

model.a = add5(2)
model.b = add10(2)
");
            Assert.AreEqual("Mozilla", model.name);
            Assert.AreEqual(7, model.a);
            Assert.AreEqual(12, model.b);
        }

        [Test]
        public void ClosurePrivateMethods()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var counter = (function() {
  var privateCounter = 0;
  function changeBy(val) {
    privateCounter += val;
  }

  return {
    increment: function() {
      changeBy(1);
    },

    decrement: function() {
      changeBy(-1);
    },

    value: function() {
      return privateCounter;
    }
  };
})();
if (changeBy !== undefined)
    throw 'changeBy is defined'
model.a = counter.value()
counter.increment();
counter.increment();
model.b = counter.value()
counter.decrement();
model.c = counter.value()
");
            Assert.AreEqual(0, model.a);
            Assert.AreEqual(2, model.b);
            Assert.AreEqual(1, model.c);
        }

        [Test]
        public void ClosureScopeChain()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var e = 10;
function sum(a){
  return function(b){
    return function(c){
      // outer functions scope
      return function(d){
        // local scope
        return a + b + c + d + e;
      }
    }
  }
}
model.a = sum(1)(2)(3)(4)
");
            Assert.AreEqual(20, model.a);
        }

        [Test]
        public void ClosureScopeChain2()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var e = 10;
function sum(a){
  return function sum2(b){
    return function sum3(c){
      // outer functions scope
      return function sum4(d){
        // local scope
        return a + b + c + d + e;
      }
    }
  }
}

if (sum2 !== undefined)
    throw 'sum2 is defined'
var sum2 = sum(1);
var sum3 = sum2(2);
var sum4 = sum3(3);
var result = sum4(4);
model.a = result
");
            Assert.AreEqual(20, model.a);
        }
    }
}
using NUnit.Framework;
using System;
using System.Linq;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class DelegateArgumentConversionTests
    {
        private class DelegateTester
        {
            public delegate void DelegateNoArgument();
            public delegate int Delegate1(int x);
            public delegate int Delegate2(int x, params int[] args);

            public DelegateNoArgument TheDelegateNoArgument => Sum;
            public Delegate1 TheDelegate1 => Sum;
            public Delegate2 TheDelegate2 => Sum;

            public int Counter1;
            public void Sum()
            {
                ++Counter1;
            }

            public int Sum(int x)
            {
                return x;
            }

            public int Sum(int x, params int[] args)
            {
                return x + args.Sum();
            }
        }

        [Test]
        public void TestDelegateCalls()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.SetValue("delegateTester", new DelegateTester());
            engine.ExecuteScript(@"
model.a = delegateTester.TheDelegate1(3)
model.b = delegateTester.TheDelegate2(5)
model.c = delegateTester.TheDelegate2(7, 5, 3)
model.d = delegateTester.Sum(7)
model.e = delegateTester.Sum(7, 6)
model.f = delegateTester.Sum(7,6,5,4,3,2,1,0)
model.g = delegateTester.Sum(7,6,5,4,3,2,1,0,'33')
model.h = delegateTester.TheDelegate2(7,6,5,4,3,2,1,0,'55')
");
            Assert.AreEqual(3, model.a);
            Assert.AreEqual(5, model.b);
            Assert.AreEqual(15, model.c);
            Assert.AreEqual(7, model.d);
            Assert.AreEqual(13, model.e);
            Assert.AreEqual(28, model.f);
            Assert.AreEqual(61, model.g);
            Assert.AreEqual(83, model.h);
        }

        [Test]
        public void TestDelegateWithNoArgument()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            var delegateTester = new DelegateTester();
            engine.SetValue("delegateTester", delegateTester);

            var test1 = 0;
            engine.SetValue("action1", new Action(() => test1 = 1));

            engine.ExecuteScript(@"
var a = delegateTester.TheDelegateNoArgument
delegateTester.TheDelegateNoArgument()
a()
action1()
");
            Assert.AreEqual(2, delegateTester.Counter1);
            Assert.AreEqual(1, test1);
        }
    }
}
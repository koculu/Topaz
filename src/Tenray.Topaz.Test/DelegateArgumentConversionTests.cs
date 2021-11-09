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
            public delegate int Delegate1(int x);
            public delegate int Delegate2(int x, params int[] args);

            public Delegate1 TheDelegate1 => Sum;
            public Delegate2 TheDelegate2 => Sum;

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
            Assert.AreEqual(model.a, 3);
            Assert.AreEqual(model.b, 5);
            Assert.AreEqual(model.c, 15);
            Assert.AreEqual(model.d, 7);
            Assert.AreEqual(model.e, 13);
            Assert.AreEqual(model.f, 28);
            Assert.AreEqual(model.g, 61);
            Assert.AreEqual(model.h, 83);
        }
    }
}
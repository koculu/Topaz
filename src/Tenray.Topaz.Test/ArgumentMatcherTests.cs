using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class ArgumentMatcherTests
    {
        [Test]
        public void TestArgumentMatch1()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.SetValue("test", new TestClass());
            engine.ExecuteScript(@"
test.Method()
test.Assert(0)
test.Method(7)
test.Assert(1, 7)
test.Method('abc')
test.Assert(2, 'abc')
test.Method(3.5)
test.Assert(1, 4, 0) // integer rounding
test.Method(44, 8)
test.Assert(3, 44.0, 8)
test.Method(55, 9, 3)
test.Assert(3, 55.0, 9, 3);
test.Method(35, 9, 3, 4)
test.Assert(3, 35.0, 9, 7);
model.a = test.Method('3.5', 6, 2, 9, 10);
");
            Assert.AreEqual(21, model.a);
        }

        public class TestClass
        {
            public int LastCall = 0;
            public int B = 0;
            public object A = null;
            public int Sum = 0;
            public void Assert(int lastCall, object a = null, int b = 0, int sum = 0)
            {
                if (!object.Equals(A, a))
                    throw new Exception($"A mismatch: {A} != {a}");
                if (lastCall != LastCall)
                    throw new Exception($"LastCaller mismatch: {LastCall} != {lastCall}");
                if (B != b)
                    throw new Exception($"B mismatch: {B} != {b}");
                if (Sum != sum)
                    throw new Exception($"Sum mismatch: {Sum} != {sum}");
            }

            public void Method()
            {
                LastCall = 0;
            }

            public void Method(int a)
            {
                LastCall = 1;
                A = a;
            }

            public void Method(string a)
            {
                LastCall = 2;
                A = a;
            }

            public int Method(double a, int b = 3, params int[] args)
            {
                A = a;
                B = b;
                LastCall = 3;
                Sum = args.Sum();
                return Sum;
            }
        }
    }
}
using NUnit.Framework;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class VariableTypeTests
    {
        [Test]
        public void TestVariableTypes()
        {
            var engine = new TopazEngine();
            engine.Options.LiteralNumbersAreConvertedToDouble = false;
            engine.Options.NumbersAreConvertedToDoubleInArithmeticOperations = false;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
model.a = 0
model.b = 1
model.c = 1.1
model.d = 2147483647
model.e = 2147483647 + 1
model.f = 2147483647 * 2147483647
model.g = -2147483647
model.h = -2147483647-1
model.i = -2147483647-2
model.j = 2147483647 * 2147483647 * 2147483647 * 2147483647
model.k = 2 ** 31
model.l = 2 ** 63
model.m = 2 ** 65
");         
            Assert.IsTrue(model.a is int);
            Assert.IsTrue(model.b is int);
            Assert.IsTrue(model.c is double);
            Assert.IsTrue(model.d is int);
            Assert.IsTrue(model.e is long);
            Assert.IsTrue(model.f is long);
            Assert.IsTrue(model.g is int);
            Assert.IsTrue(model.h is int);
            Assert.IsTrue(model.i is long);
            Assert.IsTrue(model.j is double);
            Assert.IsTrue(model.k is double);
            Assert.IsTrue(model.l is double);
            Assert.IsTrue(model.m is double);
        }
    }
}
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class LocalVariableCacheTests
    {
        [Test]
        public void TestIdentifierSharing()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
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
            dynamic model = new CaseSensitiveDynamicObject();
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
                    engine.InvokeFunction("f1", 3);
                else
                    engine.InvokeFunction("f2", 5);
                Assert.AreEqual(3, model.a);
                Assert.AreEqual(5, model.b);
            });
            Assert.AreEqual(3, model.a);
            Assert.AreEqual(5, model.b);
        }
    }
}
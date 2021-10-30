using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class MultithreadingTests
    {
        [Test]
        public void TestParallelLoop()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            engine.AddType(typeof(Parallel), "Parallel");
            engine.AddType(typeof(Action), "Action");
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var g = 0;
function f1(i) {
++g;
}
new Action(f1)
Parallel.For(0, 10000, f1)
var q = 0
Parallel.For(0, 10000, (x) => q = x)
Parallel.For(0, 10000, (x) => q = x)
model.g = g
model.q = q
");
            Assert.IsTrue(100 < model.g);
            Assert.IsTrue(100 < model.q);
        }
    }
}
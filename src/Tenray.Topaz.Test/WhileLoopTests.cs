using NUnit.Framework;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class WhileLoopTests
    {
        [Test]
        public void WhileLoop()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
let i = 0
while (i < 10) {
    ++i;
    const i = 4;
}
var j = 0
while (j < 10) {
    ++j;
if (j < 6) continue;
else if (j == 6) break;
    j += 10;
    const i = 4;
}
model.i = i
model.j = j
");
            Assert.AreEqual(10, model.i);
            Assert.AreEqual(6, model.j);
        }

        [Test]
        public void DoWhileLoop()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
let i = 0
do {
    ++i;
    const i = 4;
}
while (i < 10)
var j = 0
do {
    ++j;
if (j < 6) continue;
else if (j == 6) break;
    j += 10;
    const i = 4;
} while (j < 10)
model.i = i
model.j = j
");
            Assert.AreEqual(10, model.i);
            Assert.AreEqual(6, model.j);
        }
    }
}
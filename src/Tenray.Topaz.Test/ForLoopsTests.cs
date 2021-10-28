using NUnit.Framework;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class ForLoopsTests
    {
        [Test]
        public void ForInLoop()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
let items = [1, 2, 3]
let index = 3
for (const index in items) {
    model[index] = items[index]
}
");
            Assert.AreEqual(1, model["0"]);
            Assert.AreEqual(2, model["1"]);
            Assert.AreEqual(3, model["2"]);
        }

        [Test]
        public void ForOfLoop()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
let items = [51, 22, 33]
for (const item of items) {
    model[item] = item
}
");
            Assert.AreEqual(51, model["51"]);
            Assert.AreEqual(22, model["22"]);
            Assert.AreEqual(33, model["33"]);
        }

        [Test]
        public void ForLoopConstVarDef()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
let items = [1, 2, 3]
let index = 3
for (const index in items) {
    const a = 3
    model[index] = items[index]
}
for (const index of items) {
    const a = 3
}
for (var i = 0 ; i < 3; ++i) {
    const a = 3
}
");
            Assert.AreEqual(1, model["0"]);
            Assert.AreEqual(2, model["1"]);
            Assert.AreEqual(3, model["2"]);
        }
    }
}
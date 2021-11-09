using NUnit.Framework;
using System.Collections.Generic;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class SpreadOperatorTests
    {
        [Test]
        public void SpreadOperator()
        {
            var engine = new TopazEngine();
            dynamic model = new List<double>();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
max = (...args) => {
    for (const item of args) {
        model.Add(item)
    }
}
max(-1, 5, 11, 3)
max(...[-1, 5, 11, 3])
");
            Assert.AreEqual(new List<int>
            {
                -1, 5, 11, 3,-1, 5, 11, 3
            }, model);
        }

        [Test]
        public void SpreadArray()
        {
            var engine = new TopazEngine();
            dynamic model = new List<double>();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var arr = [1, ...[2,3], 4]
function max(...args) {
    for (const item of args) {
        model.Add(item)
    }
}
max(...arr)
");
            Assert.AreEqual(new List<int>
            {
                1,2,3,4
            }, model);
        }

        [Test]
        public void EnumerableObjectSpread()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            model.a = "aa";
            model.b = "bb";
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var c = [...model]
model.c = c
");
            Assert.AreEqual("bb", model.c[1].Value);
        }
    }
}
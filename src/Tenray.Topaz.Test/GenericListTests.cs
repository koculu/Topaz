using NUnit.Framework;
using System.Collections.Generic;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class GenericListTests
    {
        [Test]
        public void TestList1()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            var list = new List<int>()
            {
                1,2,3,4,5
            };
            engine.SetValue("list", list);
            engine.ExecuteScript(@"
model.a1 = list[0]
model.a2 = list['0']
list[1] = 8
model.a3 = list[1]
list.Add(9)
model.a4 = list[5]
model.a5 = list.Count
");
            Assert.AreEqual(1, model.a1);
            Assert.AreEqual(1, model.a2);
            Assert.AreEqual(8, model.a3);
            Assert.AreEqual(9, model.a4);
            Assert.AreEqual(6, model.a5);
        }
    }
}
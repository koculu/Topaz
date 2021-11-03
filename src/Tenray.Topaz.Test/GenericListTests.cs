using NUnit.Framework;
using System.Collections.Generic;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class GenericListTests
    {
        [Test]
        public void TestList1()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
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
            Assert.AreEqual(model.a1, 1);
            Assert.AreEqual(model.a2, 1);
            Assert.AreEqual(model.a3, 8);
            Assert.AreEqual(model.a4, 9);
            Assert.AreEqual(model.a5, 6);
        }
    }
}
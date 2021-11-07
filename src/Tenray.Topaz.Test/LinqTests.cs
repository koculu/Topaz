using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class LinqTests
    {
        [Test]
        public void TestLinq()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            engine.AddType(typeof(Enumerable), "Enumerable");
            engine.SetValue("model", model);
            var items = Enumerable.Range(1, 100).Select(x => new
            {
                Name = "item " + x,
                Index = x
            }).ToArray();
            engine.SetValue("items", items);
            engine.ExecuteScript(@"
var query = Enumerable.Where(items, (x) => x.Index % 2 == 1);
query = Enumerable.Select(query, (x) => x.Name);
model.a = Enumerable.ToArray(query);
");
            Assert.AreEqual(50, model.a.Length);
            Assert.AreEqual("item 1", model.a[0]);
            Assert.AreEqual("item 21", model.a[10]);
        }
    }
}
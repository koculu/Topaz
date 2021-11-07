using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class LinqTests
    {
        [Test]
        public void TestStaticLinq()
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
query = Enumerable.Select(query, (x, i) => x.Name);
model.a = Enumerable.ToArray(query);
");
            Assert.AreEqual(50, model.a.Length);
            Assert.AreEqual("item 1", model.a[0]);
            Assert.AreEqual("item 21", model.a[10]);
        }

        [Test]
        public void TestLinqViaExtension()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            engine.AddExtensionMethods(typeof(Enumerable));
            engine.SetValue("model", model);
            var items = Enumerable.Range(1, 100).Select(x => new
            {
                Name = "item " + x,
                Index = x
            }).ToArray();
            engine.SetValue("items", items);
            engine.ExecuteScript(@"
model.a = 
items
    .Where((x) => x.Index % 2 == 1)
    .Select((x, i) => x.Name + ' : ' + i)
    .ToArray();
");
            Assert.AreEqual(50, model.a.Length);
            Assert.AreEqual("item 1 : 0", model.a[0]);
            Assert.AreEqual("item 21 : 10", model.a[10]);
        }

        [Test]
        public void TestCustomExtensionMethod()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            engine.AddExtensionMethods(typeof(MyExtensions));
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
model.a = 'Hello world, from Topaz Script!'.WordCount();
");
            Assert.AreEqual(5, model.a);
        }
    }

    public static class MyExtensions
    {
        public static int WordCount(this string str)
        {
            return str.Split(new char[] { ' ', '.', '?' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
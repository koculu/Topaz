using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class LinqTests
    {
        [Test]
        public void TestStaticLinq()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
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
            dynamic model = new JsObject();
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
            dynamic model = new JsObject();
            engine.AddExtensionMethods(typeof(MyExtensions));
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
model.a = 'Hello world, from Topaz Script!'.WordCount();
model.b = 'Hello world, from Topaz Script!'.WordCountGeneric(3);
model.c = 'Hello world, from Topaz Script!'.WordCountGeneric(5.2);
var k = 3
model.d = 'Hello world, from Topaz Script!'.GenericArguments(k.GetType()).WordCountGeneric(7.2)
");
            Assert.AreEqual(5, model.a);
            Assert.AreEqual(8, model.b);
            Assert.AreEqual(10, model.c);
            Assert.AreEqual(12, model.d);
        }

        [Test]
        public void TestLinqGroupByViaExtension()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.AddExtensionMethods(typeof(Enumerable));
            engine.SetValue("model", model);
            var items = Enumerable.Range(1, 100).Select(x => new
            {
                Name = "item " + x,
                Index = x
            }).ToArray();
            engine.SetValue("items", items);
            engine.SetValue("JSON", new JSONObject());
            engine.ExecuteScript(@"
JSON.stringify(1)
var a =
items
    .GroupBy((x) => x.Name.Substring(0, 6))
    .Select((x) => 
{ 
    return { 
        key: x.Key,
        count: x.Sum((y) => y.Index)
    }
})
.ToArray();

model.a = JSON.parse(JSON.stringify(a));
");
            Console.WriteLine(model.a);
            Assert.AreEqual("item 6", model.a[5].key);
            Assert.AreEqual(651, model.a[5].count);
            Assert.AreEqual("item 7", model.a[6].key);
            Assert.AreEqual(752, model.a[6].count);
        }

        [Test]
        public void TestDefineGenericMethodArgumentTypes()
        {
            var engine = new TopazEngine();
            engine.AddNamespace("System");
            dynamic model = new JsObject();
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
    .GenericArguments(System.Object, System.String)
    .Select((x, i) => x.Name + ' : ' + i)
    .GenericArguments(System.String)
    .ToArray();
");
            Assert.AreEqual(typeof(string[]), model.a.GetType());
            Assert.AreEqual(50, model.a.Length);
            Assert.AreEqual("item 1 : 0", model.a[0]);
            Assert.AreEqual("item 21 : 10", model.a[10]);
        }

        [Test]
        public void TestDefineStaticGenericMethodArgumentTypes()
        {
            var engine = new TopazEngine();
            engine.AddNamespace("System");
            dynamic model = new JsObject();
            engine.AddType(typeof(Enumerable), "Enumerable");
            engine.SetValue("model", model);
            var items = Enumerable.Range(1, 100).Select(x => new
            {
                Name = "item " + x,
                Index = x
            }).ToArray();
            engine.SetValue("items", items);
            engine.ExecuteScript(@"
var query = Enumerable.Where(items, (x) => x.Index % 2 == 1)
query = Enumerable
    .GenericArguments(System.Object, System.String)
    .Select(query, (x, i) => x.Name)
model.a = Enumerable
    .GenericArguments(System.String)
    .ToArray(query)
");
            Assert.AreEqual(typeof(string[]), model.a.GetType());
            Assert.AreEqual(50, model.a.Length);
            Assert.AreEqual("item 1", model.a[0]);
            Assert.AreEqual("item 21", model.a[10]);
        }

        [Test]
        public void TestToDictionaryWithGenericArguments()
        {
            var engine = new TopazEngine();
            engine.AddNamespace("System");
            dynamic model = new JsObject();
            engine.AddExtensionMethods(typeof(Enumerable));
            engine.SetValue("model", model);
            var items = Enumerable.Range(1, 100).Select(x => new
            {
                Name = "item " + x,
                Index = x
            }).ToArray();
            engine.SetValue("items", items);
            engine.ExecuteScript(@"
model.a = items.ToDictionary(x => x.Index, y => y.Name)

model.b = items
    .GenericArguments(System.Object, System.Int32, System.String)
    .ToDictionary(x => x.Index, y => y.Name)

model.c = items
    .GenericArguments(items[0].GetType(), System.Double, System.String)
    .ToDictionary(x => x.Index, y => y.Name)
");
            Assert.AreEqual(typeof(Dictionary<object, object>), model.a.GetType());
            Assert.AreEqual(typeof(Dictionary<int, string>), model.b.GetType());
            Assert.AreEqual(typeof(Dictionary<double, string>), model.c.GetType());
        }

        public class TestThisType
        {
            public string Name { get; set; }

            public int Index { get; set; }
        }

        [Test]
        public void TestSelectGenericArgumentFromThisType()
        {
            var engine = new TopazEngine();
            engine.AddNamespace("System");
            dynamic model = new JsObject();
            engine.AddExtensionMethods(typeof(Enumerable));
            engine.SetValue("model", model);
            var items = Enumerable.Range(1, 100).Select(x => new TestThisType
            {
                Name = "item " + x,
                Index = x
            }).ToArray();
            engine.SetValue("items", items);
            engine.ExecuteScript(@"
model.a = 
items
    .Where((x) => x.Index % 2 == 1)
    .ToArray();
");
            Assert.AreEqual(typeof(TestThisType[]), model.a.GetType());
            Assert.AreEqual(50, model.a.Length);
            Assert.AreEqual("item 1", model.a[0].Name);
            Assert.AreEqual("item 21", model.a[10].Name);
        }

        [Test]
        public void TestToDictionaryWithGenericArguments2()
        {
            var engine = new TopazEngine();
            engine.AddNamespace("System");
            dynamic model = new JsObject();
            engine.AddExtensionMethods(typeof(Enumerable));
            engine.SetValue("model", model);
            var items = Enumerable.Range(1, 100).Select(x => new TestThisType
            {
                Name = "item " + x,
                Index = x
            }).ToArray();
            engine.SetValue("items", items);
            engine.ExecuteScript(@"
model.a = items.ToDictionary(x => x.Index, y => y.Name)

model.b = items
    .GenericArguments(System.Object, System.Int32, System.String)
    .ToDictionary(x => x.Index, y => y.Name)

model.c = items
    .GenericArguments(items[0].GetType(), System.Double, System.String)
    .ToDictionary(x => x.Index, y => y.Name)

model.d = items
    .ToDictionary(x => x.Index)
");
            Assert.AreEqual(typeof(Dictionary<object, object>), model.a.GetType());
            Assert.AreEqual(typeof(Dictionary<int, string>), model.b.GetType());
            Assert.AreEqual(typeof(Dictionary<double, string>), model.c.GetType());
            Assert.AreEqual(typeof(Dictionary<object, TestThisType>), model.d.GetType());
        }
    }

    public static class MyExtensions
    {
        public static int WordCount(this string str)
        {
            return str.Split(new char[] { ' ', '.', '?' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static int WordCountGeneric<T1>(this string str, T1 a)
        {
            var b = Convert.ToInt32(a);
            return str.Split(new char[] { ' ', '.', '?' },
                             StringSplitOptions.RemoveEmptyEntries).Length + b;
        }
    }
}
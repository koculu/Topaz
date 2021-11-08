using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class ConstructorTests
    {
        [Test]
        public void DateTimeConstruction()
        {
            var engine = new TopazEngine();
            engine.AddType<DateTime>("DateTime");
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
model.a = new DateTime(2021, 7, 21, 5, 5, 5, 'uTC')
model.a = model.a.AddTicks(5555);
model.b = DateTime.Parse('2021-11-11').ToString('dd/MM/yyyy HH:mm:ss')
model.c = (DateTime.Now.GetType()).ToString()
");
            Assert.AreEqual(
                new DateTime(2021, 7, 21, 5, 5, 5, DateTimeKind.Utc)
                .AddTicks(5555), model.a);
            Assert.AreEqual(
                "11/11/2021 00:00:00", model.b);
            Assert.AreEqual(
                "System.DateTime", model.c);
        }

        [Test]
        public void GenericTypeConstruction()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            model["int"] = typeof(int);
            model["string"] = typeof(string);
            engine.SetValue("model", model);
            engine.AddType(typeof(Dictionary<,>), "GenericDictionary");

            engine.ExecuteScript(@"
var dic = model.dic = new GenericDictionary(model.string, model.int)
dic.Add('hello', 1)
dic.Add('dummy', 0)
dic.Add('world', 2)
dic.Remove('dummy')
");
            Assert.AreEqual(1, model.dic["hello"]);
            Assert.AreEqual(2, model.dic["world"]);
            Assert.IsFalse(model.dic.ContainsKey("dummy"));
        }
    }
}
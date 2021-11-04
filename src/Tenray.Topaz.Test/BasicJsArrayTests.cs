using NUnit.Framework;
using System;
using System.Collections;
using System.Text.Json;
using Tenray.Topaz.API;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class BasicJsArrayTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArray1(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = []
var b = [4,5,6]
a[0] = 'abc'
a[3] = 3
model.js = a
model.x = a[55]
model.y = a.at(3)
model.z = a.concat(b)
model.p = model.z.concat()
model.p.push(7,8,9,10)
model.p.push(11)
model.q = model.p.pop()
model.p.push(12,13)
model.r = model.p.shift()
");
            var js = model.js;
            var json = JsonSerializer.Serialize<JsArray>(js);
            Assert.IsTrue(json.StartsWith("["));
            Console.WriteLine(json);
            dynamic deserialized;
            if (useThreadSafeJsObjects)
            {
                deserialized = JsonSerializer.Deserialize<JsArray>(json);
                Assert.AreEqual(js, deserialized);
            }
            else
            {
                deserialized = JsonSerializer.Deserialize<JsArray>(json);
                Assert.AreEqual(js, deserialized);
            }

            Assert.AreEqual("abc", js[0]);
            Assert.AreEqual(null, js[1]);
            Assert.AreEqual(null, js[2]);
            Assert.AreEqual(3, js[3]);
            Assert.AreEqual(Undefined.Value, js[4]);
            Assert.AreEqual(null, model.x);
            Assert.AreEqual(3, model.y);
            Assert.AreEqual("[\"abc\",null,null,3,4,5,6]", 
                JsonSerializer.Serialize<JsArray>((model.z)));
            Assert.AreEqual("[null,null,3,4,5,6,7,8,9,10,12,13]",
                JsonSerializer.Serialize<JsArray>((model.p)));
            Assert.AreEqual(11, model.q);
            Assert.AreEqual("abc", model.r);

            engine.ExecuteScript(@"
model.u = model.p.length
model.p.length = 9
model.p.shift()
model.p.shift()
");
            Assert.AreEqual(12, model.u);
            Assert.AreEqual("[3,4,5,6,7,8,9]",
                JsonSerializer.Serialize<JsArray>((model.p)));

            engine.ExecuteScript(@"
model.p.reverse()
");
            Assert.AreEqual("[9,8,7,6,5,4,3]",
                JsonSerializer.Serialize<JsArray>((model.p)));
        }
    }
}
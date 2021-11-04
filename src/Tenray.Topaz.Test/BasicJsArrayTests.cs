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

            Assert.AreEqual(null, js[0]);
            Assert.AreEqual(null, js[1]);
            Assert.AreEqual(null, js[2]);
            Assert.AreEqual(3, js[3]);
            Assert.AreEqual(Undefined.Value, js[4]);
            Assert.AreEqual(null, model.x);
            Assert.AreEqual(3, model.y);
            Assert.AreEqual("[null,null,null,3,4,5,6]", 
                JsonSerializer.Serialize<JsArray>((model.z)));
            Assert.AreEqual("[null,null,null,3,4,5,6,7,8,9,10,12,13]",
                JsonSerializer.Serialize<JsArray>((model.p)));
            Assert.AreEqual(11, model.q);
        }
    }
}
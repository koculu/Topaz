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
a[3] = 3
model.js = a;
model.x = a[55];
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
        }
    }
}
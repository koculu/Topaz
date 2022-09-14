using NUnit.Framework;
using System.Collections.Generic;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public sealed class NamespaceTests
    {
        [Test]
        public void TestSingleLevelNamespace()
        {
            var engine = new TopazEngine();
            engine.AddNamespace("System", new HashSet<string>
            {
                "System.Int32"
            }, false);

            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = System.Int32.Parse('3')
model.a = a
model.b = 9
model.b = System.Collections.ArrayList
"); 
            Assert.AreEqual(3, model.a);
            Assert.IsNull(model.b);
        }

        [Test]
        public void TestEntireNamespace()
        {
            var engine = new TopazEngine();
            engine.AddNamespace("System", null, true);
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = new System.Collections.Generic.Dictionary(System.String, System.Int32)
a.Add('key1', 13)
model.a = a
");
            Assert.AreEqual(13, model.a["key1"]);
        }

        [Test]
        public void TestRestrictedNamespace()
        {
            var engine = new TopazEngine();
            var whitelist = new HashSet<string> {
                "System.Int32",
                "System.String",
                "System.Collections.Generic.Dictionary"
            };

            engine.AddNamespace("System", whitelist, true);
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = new System.Collections.Generic.Dictionary(System.String, System.Int32)
a.Add('key1', 13)
model.a = a
model.b = System.Double
");
            Assert.AreEqual(13, model.a["key1"]);
            Assert.IsNull(model.b);
        }
    }
}
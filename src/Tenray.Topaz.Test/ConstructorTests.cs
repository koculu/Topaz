using NUnit.Framework;
using System;
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
model.b = DateTime.Parse('2021-11-11').ToString()
model.c = (typeof DateTime.Now).ToString()
");
            Assert.AreEqual(
                new DateTime(2021, 7, 21, 5, 5, 5, DateTimeKind.Utc)
                .AddTicks(5555), model.a);
            Assert.AreEqual(
                "11/11/2021 00:00:00", model.b);
            Assert.AreEqual(
                "System.DateTime", model.c);
        }
    }
}
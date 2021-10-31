using NUnit.Framework;
using System.Reflection;
using Tenray.Topaz.Options;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Test
{
    public class SecurityTests
    {
        [Test]
        public void TryToUseReflectionAPI()
        {
            var engine = new TopazEngine();
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
try {
    var m = model.GetType().Assembly
    model.m = m
    model.a = m.GetType('System.DateTime')
}
catch(err) {
    model.b = err
}
");
            Assert.IsNull(model.a);
            Assert.IsInstanceOf<Assembly>(model.m);
            Assert.IsInstanceOf<TopazException>(model.b);
        }

        [Test]
        public void EnableReflectionAPI()
        {
            var engine = new TopazEngine();
            engine.Options.SecurityPolicy = SecurityPolicy.EnableReflection;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
try {
    var m = model.GetType().Assembly
    model.m = m
    model.a = m.GetType('System.DateTime')
}
catch(err) {
    throw err
}
");
            Assert.IsNull(model.a);
            Assert.IsInstanceOf<Assembly>(model.m);
        }
    }
}
﻿using NUnit.Framework;
using System;
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
            engine.SetValue("args", new[] { typeof(DateTime) });
            engine.ExecuteScript(@"
try {
    var m = model.GetType().Assembly
    model.m = m
    model.a = m.GetType('System.DateTime')
    var activator = m.GetType('System.Activator');
    var createInstance = activator.GetMember('CreateInstance')[3]
    var newDateTime = createInstance.Invoke(null, args);
    model.newDateTime = newDateTime
}
catch(err) {
    model.b = err
}
");
            Assert.IsNull(model.a);
            Assert.IsInstanceOf<Assembly>(model.m);
            Assert.IsInstanceOf<TopazException>(model.b);
            Assert.IsNull(model.newDateTime);
        }

        [Test]
        public void EnableReflectionAPI()
        {
            var engine = new TopazEngine();
            engine.Options.SecurityPolicy = SecurityPolicy.EnableReflection;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.SetValue("args", new[] { typeof(DateTime) });
            engine.ExecuteScript(@"
try {
    var m = (typeof '').GetType().Assembly
    model.m = m
    model.a = m.GetType('System.DateTime')
    var activator = m.GetType('System.Activator');
    var createInstance = activator.GetMember('CreateInstance')[3]
    var newDateTime = createInstance.Invoke(null, args);
    model.newDateTime = newDateTime
}
catch(err) {
    throw err
}
");
            Assert.IsNotNull(model.a);
            Assert.AreEqual(typeof(DateTime), model.a);
            Assert.AreEqual(new DateTime(), model.newDateTime);
            Assert.IsInstanceOf<Assembly>(model.m);
        }
    }
}
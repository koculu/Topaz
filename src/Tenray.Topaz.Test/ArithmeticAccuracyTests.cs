using NUnit.Framework;
using System.Collections;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class ArithmeticAccuracyTests
    {
        [Test]
        public void TestArithmeticComparison()
        {
            var engine = new TopazEngine();
            engine.Options.LiteralNumbersAreConvertedToDouble = false;
            engine.Options.NumbersAreConvertedToDoubleInArithmeticOperations = false;
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = []
var i = 0
a[i++] = 3 == 3.0
a[i++] = (3+2) == (3.0 + 2.0)
a[i++] = (3+2) === (3.0 + 2.0)
a[i++] = (3*2) != (3.0 * 7.0)
a[i++] = (3*2) !== (3.0 * 7.0)
a[i++] = '3' !== 3
a[i++] = '3' == 3

a[i++] = true != false
a[i++] = true !== false

a[i++] = !(null != null)
a[i++] = !(null !== null)

a[i++] = null == null
a[i++] = null === null

a[i++] = !('abc' != 'abc')
a[i++] = !('abc' !== 'abc')

a[i++] = !!('abc' != 'cba')
a[i++] = !!('abc' !== 'cba')

a[i++] = 'abc' == 'abc'
a[i++] = 'abc' === 'abc'

var j = 0
a[i++] = (++j) == 1
a[i++] = (j++) == 1
a[i++] = j == 2

model.a = a
");
           
            foreach (var item in (IEnumerable)(model.a))
                Assert.IsTrue((bool)item);
        }
    }
}
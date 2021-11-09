using NUnit.Framework;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Test
{
    public class TemplateLiteralTests
    {
        [Test]
        public void TaggedTemplate()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
let person = 'Mike';
let age = 28;

function myTag(strings, personExp, ageExp) {
  let str0 = strings[0]; // That 
  let str1 = strings[1]; //  is a 
  let str2 = strings[2]; // .

  let ageStr;
            if (ageExp > 99)
            {
                ageStr = 'centenarian';
            }
            else
            {
                ageStr = 'youngster';
            }

            // We can even return a string built using a template literal
            return `${ str0}${ personExp}${ str1}${ ageStr}${ str2}`;
        }

        let output = myTag`That ${ person
    } is a ${ age
}.`;

model.a = myTag`That ${ person } is a ${ age }.`
model.b = myTag`That ${ person } is a ${ 157 }.`
");
            Assert.AreEqual("That Mike is a youngster.", model.a);
            Assert.AreEqual("That Mike is a centenarian.", model.b);
        }

        [Test]
        public void TemplateArithmetic()
        {
            var engine = new TopazEngine();
            dynamic model = new JsObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
model.a = `2 + 3 = ${2+3}`
");
            Assert.AreEqual("2 + 3 = 5", model.a);;
        }
    }
}
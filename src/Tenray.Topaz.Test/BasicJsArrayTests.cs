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
                JsonSerializer.Serialize<JsArray>(model.z));
            Assert.AreEqual("[null,null,3,4,5,6,7,8,9,10,12,13]",
                JsonSerializer.Serialize<JsArray>(model.p));
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
                JsonSerializer.Serialize<JsArray>(model.p));

            engine.ExecuteScript(@"
model.p.reverse()
");
            Assert.AreEqual("[9,8,7,6,5,4,3]",
                JsonSerializer.Serialize<JsArray>(model.p));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArray2(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = [{nested: 4}, [9,8,[7,6]], 4]
model.z = a.concat([1,2,3], [4,5,6])
");
            var json = JsonSerializer.Serialize<JsArray>(model.z);
            Console.WriteLine(json);
            var deserialized = JsonSerializer.Deserialize<JsArray>(json);
            Assert.AreEqual(typeof(JsObject), deserialized[0].GetType());
            Assert.AreEqual(typeof(JsArray), deserialized[1].GetType());
            Assert.AreEqual(typeof(JsArray), deserialized[1][2].GetType());
            Assert.AreEqual(model.z, deserialized);
            Assert.AreEqual("[{\"nested\":4},[9,8,[7,6]],4,1,2,3,4,5,6]",
                json);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayIndexOf(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = [1,2,3,4,5,2]
model.b = a.indexOf(3)
model.c = a.indexOf(3,3)
model.d = a.indexOf(2)
model.e = a.indexOf(2, 2)
model.f = a.indexOf(2, -2)
");
            Assert.AreEqual(2, model.b);
            Assert.AreEqual(-1, model.c);
            Assert.AreEqual(1, model.d);
            Assert.AreEqual(5, model.e);
            Assert.AreEqual(5, model.f);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayLastIndexOf(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = [1,2,3,4,5,2]
model.b = a.lastIndexOf(3)
model.c = a.lastIndexOf(3,3)
model.d = a.lastIndexOf(2)
model.e = a.lastIndexOf(2, 2)
model.f = a.lastIndexOf(2, -2)
");
            Assert.AreEqual(2, model.b);
            Assert.AreEqual(2, model.c);
            Assert.AreEqual(5, model.d);
            Assert.AreEqual(1, model.e);
            Assert.AreEqual(1, model.f);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayIncludes(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = [1,2,3,4,5,2]
model.b = a.includes(3)
model.c = a.includes(3,3)
model.d = a.includes(2)
model.e = a.includes(2, 2)
model.f = a.includes(2, -2)
");
            Assert.IsTrue(model.b);
            Assert.IsFalse(model.c);
            Assert.IsTrue(model.d);
            Assert.IsTrue(model.e);
            Assert.IsTrue(model.f);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayUnshift(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = [3]
a.unshift(2)
a.unshift(1)
a.unshift(6,5,4)
model.a = a
model.b = a.toString()
");
            Assert.AreEqual("[6,5,4,1,2,3]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("6,5,4,1,2,3", model.b);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayValuesIterator(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const array1 = ['a', 'b', 'c']
const iterator = array1.values()
let a = []
for (const value of iterator) {
  a.push(value)
}
model.a = a
let b = []
for (const value of array1.keys()) {
  b.push(value)
}
model.b = b
");
            Assert.AreEqual("[\"a\",\"b\",\"c\"]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("[0,1,2]",
                JsonSerializer.Serialize<JsArray>(model.b));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayValuesSort(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var a = [4, 2, 5, 1, 3];
a.sort(function(a, b) {
  return a - b;
});
model.a = a
");
            Assert.AreEqual("[1,2,3,4,5]",
                JsonSerializer.Serialize<JsArray>(model.a));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayJoin(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const elements = ['Fire', 'Air', 'Water']
model.a = elements.join()
model.b = elements.join('')
model.c = elements.join('-')
");
            Assert.AreEqual("Fire,Air,Water", model.a);
            Assert.AreEqual("FireAirWater", model.b);
            Assert.AreEqual("Fire-Air-Water", model.c);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArraySome(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const array = [1, 2, 3, 4, 5]
const even = (element) => element % 2 === 0
model.a = array.some(even)

const TRUTHY_VALUES = [true, 'true', 1];

function getBoolean(value) {
  'use strict';

  if (typeof value === 'string') {
    value = value.ToLower().Trim();
  }

  return TRUTHY_VALUES.some(function(t) {
    return t === value;
  });
}

model.b = getBoolean(false);   // false
model.c = getBoolean('false'); // false
model.d = getBoolean(1);       // true
model.e = getBoolean('true');  // true
");
            Assert.IsTrue(model.a);
            Assert.IsFalse(model.b);
            Assert.IsFalse(model.c);
            Assert.IsTrue(model.d);
            Assert.IsTrue(model.e);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayEntries(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const a = [5,6,7]
const b = []
for (const [index, element] of a.entries()) {
  b.push(index)
  b.push(element)
}
model.b = b
");
            Assert.AreEqual("[0,5,1,6,2,7]",
                JsonSerializer.Serialize<JsArray>(model.b));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayEvery(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const isBelowThreshold = (currentValue) => currentValue < 40
const isAboveThreshold = (currentValue) => currentValue > 27
const array1 = [34, 30, 39, 29, 10, 13]
model.a = array1.every(isBelowThreshold)
model.b = array1.every(isAboveThreshold)

function isSubset(array1, array2) {
  return array2.every(function (element) {
    return array1.includes(element)
  })
}

model.c = isSubset([1, 2, 3, 4, 5, 6, 7], [5, 7, 6]) // true
model.d = isSubset([1, 2, 3, 4, 5, 6, 7], [5, 8, 7]) // false
");
            Assert.IsTrue(model.a);
            Assert.IsFalse(model.b);
            Assert.IsTrue(model.c);
            Assert.IsFalse(model.d);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayFill(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const array1 = [1, 2, 3, 4];
model.a = array1.fill(0, 2, 4).concat()
model.b = array1.fill(5, 1).concat()
model.c = array1.fill(6).concat()
model.d = array1
");
            Assert.AreEqual("[1,2,0,0]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("[1,5,5,5]",
                JsonSerializer.Serialize<JsArray>(model.b));
            Assert.AreEqual("[6,6,6,6]",
                JsonSerializer.Serialize<JsArray>(model.c));
            Assert.AreEqual("[6,6,6,6]",
                JsonSerializer.Serialize<JsArray>(model.d));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayCopyWithin(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
model.a = [1, 2, 3, 4, 5].copyWithin(-2)
model.b = [1, 2, 3, 4, 5].copyWithin(0, 3)
model.c = [1, 2, 3, 4, 5].copyWithin(0, 3, 4)
model.d = [1, 2, 3, 4, 5].copyWithin(-2, -3, -1)
");
            Assert.AreEqual("[1,2,3,1,2]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("[4,5,3,4,5]",
                JsonSerializer.Serialize<JsArray>(model.b));
            Assert.AreEqual("[4,2,3,4,5]",
                JsonSerializer.Serialize<JsArray>(model.c));
            Assert.AreEqual("[1,2,3,3,4]",
                JsonSerializer.Serialize<JsArray>(model.d));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayForeach(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
var sum = 0;
[1, 2, 3, 4, 5].forEach((x) => sum += x)
model.sum = sum
");
            Assert.AreEqual(15, model.sum);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayMap(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
model.a = [1, 2, 3, 4, 5].map((x) => x * 2)
model.b = [1, 2, 3, 4, 5].map((x,i) => x * i)
model.c = [1, 2, 3, 4, 5].map((x,i,arr) => arr[i] * x * i)
");
            Assert.AreEqual("[2,4,6,8,10]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("[0,2,6,12,20]",
                JsonSerializer.Serialize<JsArray>(model.b));
            Assert.AreEqual("[0,4,18,48,100]",
                JsonSerializer.Serialize<JsArray>(model.c));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayReduce(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const getMax = (a, b) => a > b ? a : b;
model.a = [1, 100].reduce(getMax, 50);
model.b = [50].reduce(getMax, 10);
// callback is invoked once for element at index 1
model.c = [1, 100].reduce(getMax);
model.d = [50].reduce(getMax);
model.e = [].reduce(getMax, 1);
try {
    [].reduce(getMax);
}
catch(err) {
    model.f = err;
}
");
            Assert.AreEqual(100, model.a);
            Assert.AreEqual(50, model.b);
            Assert.AreEqual(100, model.c);
            Assert.AreEqual(50, model.d);
            Assert.AreEqual(1, model.e);
            Assert.AreEqual(typeof(TopazException), model.f.InnerException.GetType());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayReduceRight(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const getMax = (a, b) => a > b ? a : b;
model.a = [1, 100].reduceRight(getMax, 50);
model.b = [50].reduceRight(getMax, 10);
// callback is invoked once for element at index 0
model.c = [1, 100].reduceRight(getMax);
model.d = [50].reduceRight(getMax);
model.e = [].reduceRight(getMax, 1);
try {
    [].reduceRight(getMax);
}
catch(err) {
    model.f = err;
}
");
            Assert.AreEqual(100, model.a);
            Assert.AreEqual(50, model.b);
            Assert.AreEqual(100, model.c);
            Assert.AreEqual(50, model.d);
            Assert.AreEqual(1, model.e);
            Assert.AreEqual(typeof(TopazException), model.f.InnerException.GetType());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayFilter(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const array = [-3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13];
function isPrime(num) {
  for (let i = 2; num > i; i++) {
    if (num % i == 0) {
      return false;
    }
  }
  return num > 1;
}

model.a = array.filter(isPrime)
");
            Assert.AreEqual("[2,3,5,7,11,13]",
                JsonSerializer.Serialize<JsArray>(model.a));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayFind(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const inventory = [
  {name: 'apples', quantity: 2},
  {name: 'bananas', quantity: 0},
  {name: 'cherries', quantity: 5}
];

function isCherries(fruit) {
  return fruit.name === 'cherries';
}
function isCherries2(fruit, index) {
  return inventory[index].name === 'cherries';
}
function isCherries3(fruit, index, arr) {
  return arr[index].name === 'bananas';
}

model.a = inventory.find(isCherries)
model.b = inventory.find(isCherries2)
model.c = inventory.find(isCherries3)
");
            Assert.AreEqual("cherries", model.a.name);
            Assert.AreEqual(5, model.a.quantity);
            Assert.AreEqual("cherries", model.b.name);
            Assert.AreEqual(5, model.b.quantity);
            Assert.AreEqual("bananas", model.c.name);
            Assert.AreEqual(0, model.c.quantity);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayFindIndex(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const inventory = [
  {name: 'apples', quantity: 2},
  {name: 'bananas', quantity: 0},
  {name: 'cherries', quantity: 5}
];

function isCherries(fruit) {
  return fruit.name === 'cherries';
}
function isCherries2(fruit, index) {
  return inventory[index].name === 'cherries2';
}
function isCherries3(fruit, index, arr) {
  return arr[index].name === 'bananas';
}

model.a = inventory.findIndex(isCherries)
model.b = inventory.findIndex(isCherries2)
model.c = inventory.findIndex(isCherries3)
");
            Assert.AreEqual(2, model.a);
            Assert.AreEqual(-1, model.b);
            Assert.AreEqual(1, model.c);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayFlat(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
const a = [0, 1, 2, [3, 4]]
model.a = a.flat()
const b = [0, 1, 2, [[[3, 4]]]]
model.b = b.flat(2)

const arr1 = [1, 2, [3, 4]];
model.c = arr1.flat();
const arr2 = [1, 2, [3, 4, [5, 6]]];
model.d = arr2.flat();
const arr3 = [1, 2, [3, 4, [5, 6]]];
model.e = arr3.flat(2);
const arr4 = [1, 2, [3, 4, [5, 6, [7, 8, [9, 10]]]]];
model.f = arr4.flat(99);
");
            Assert.AreEqual("[0,1,2,3,4]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("[0,1,2,[3,4]]",
                JsonSerializer.Serialize<JsArray>(model.b));

            Assert.AreEqual("[1,2,3,4]",
                JsonSerializer.Serialize<JsArray>(model.c));
            Assert.AreEqual("[1,2,3,4,[5,6]]",
                JsonSerializer.Serialize<JsArray>(model.d));
            Assert.AreEqual("[1,2,3,4,5,6]",
                JsonSerializer.Serialize<JsArray>(model.e));
            Assert.AreEqual("[1,2,3,4,5,6,7,8,9,10]",
                JsonSerializer.Serialize<JsArray>(model.f));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArrayFlatMap(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
let arr1 = [1, 2, 3, 4];
model.a = arr1.map(x => [x * 2]);
model.b = arr1.flatMap(x => [x * 2]);
model.c = arr1.flatMap(x => [[x * 2]]);

let a = [5, 4, -3, 20, 17, -33, -4, 18]
//       |\  \  x   |  | \   x   x   |
//      [4,1, 4,   20, 16, 1,       18]

model.d = a.flatMap( (n) =>
  (n < 0) ?      [] :
  (n % 2 == 0) ? [n] :
                 [n-1, 1]
)

");
            Assert.AreEqual("[[2],[4],[6],[8]]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("[2,4,6,8]",
                JsonSerializer.Serialize<JsArray>(model.b));
            Assert.AreEqual("[[2],[4],[6],[8]]",
                JsonSerializer.Serialize<JsArray>(model.c));
            Assert.AreEqual("[4,1,4,20,16,1,18]",
                JsonSerializer.Serialize<JsArray>(model.d));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArraySlice(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
let fruits = ['Banana', 'Orange', 'Lemon', 'Apple', 'Mango']
model.a = fruits.slice(1, 3)
model.b = [1,2,3,4,5].slice(-3,-1)
");
            Assert.AreEqual("[\"Orange\",\"Lemon\"]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("[3,4]",
                JsonSerializer.Serialize<JsArray>(model.b));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestJsArraySplice(bool useThreadSafeJsObjects)
        {
            var engine = new TopazEngine();
            engine.Options.NoUndefined = true;
            engine.Options.UseThreadSafeJsObjects = useThreadSafeJsObjects;
            dynamic model = new CaseSensitiveDynamicObject();
            engine.SetValue("model", model);
            engine.ExecuteScript(@"
model.a = [1,2,3,4,5]
model.b = model.a.splice(1, 2)
model.c = [1,2,3,4,5]
model.d = model.c.splice(-4,-2,6,7,8)
model.e = [1,2,3,4,5,6,7,8,9,10]
model.f = model.e.splice(-3)
");
            Assert.AreEqual("[1,4,5]",
                JsonSerializer.Serialize<JsArray>(model.a));
            Assert.AreEqual("[2,3]",
                JsonSerializer.Serialize<JsArray>(model.b));
            Assert.AreEqual("[1,6,7,8,2,3,4,5]",
                JsonSerializer.Serialize<JsArray>(model.c));
            Assert.AreEqual("[]",
                JsonSerializer.Serialize<JsArray>(model.d));
            Assert.AreEqual("[1,2,3,4,5,6,7]",
                JsonSerializer.Serialize<JsArray>(model.e));
            Assert.AreEqual("[8,9,10]",
                JsonSerializer.Serialize<JsArray>(model.f));
        }
    }
}
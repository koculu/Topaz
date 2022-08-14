# Topaz
Multithreaded Javascript Engine for .NET

[![Download](https://img.shields.io/badge/download-Topaz-blue)](https://www.nuget.org/packages/Topaz/)

## Why another Javascript Engine?

1. Existing Javascript Engines run scripts in single-thread. That is acceptable for desktop environments but not pleasing for server-side execution. Topaz does not restrict multi-thread usage.

2. Syntax level options which simplify script language is necessary to ease scripting support of many products.
With Topaz, you can turn off 'undefined', control asignment scope of variables and disable null reference exceptions without breaking the flow and much more.

3. The scripting engine should be lock free.

4. It must be blazingly fast in both execution and load.

## What kind of engine Topaz is?
Topaz does not convert Javascript code into machine code directly. It parses abstract syntax tree and executes instructions in the .NET runtime.

The Javascript engines like v8 convert Javascript into machine code.
There are v8 wrappers like [ClearScript](https://github.com/microsoft/ClearScript) to make use of v8 functionality in the .NET runtime.

However, that process requires marshaling and locking and also requires a lot of resources. In server-side applications using such an engine does not perform well.

Topaz will not be a JS engine that supports entire Javascript runtime features, but it will be a lightweight script interpreter which provides the full power of .NET runtime into Javascript language.

Topaz Runtime is actually the .NET runtime, therefore every variable written in the script is a .NET type. if you write `const a = 3.0` the `a` variable's type will be double and so on.

## What will be supported in Topaz?

Topaz will support everything that will make benefits.

For example, Javascript Array is very useful for data operations and it is fully implemented and available.

The latest Javascript Language features are mostly implemented.

For now, there is no class support, because it is not intended to write big applications using Topaz. In the future, we might add class support.

## How is the performance of Topaz?

Overall Topaz's performance is better than Clearscript and Jint's. However, there might be different scenarios in which some slower engines can perform better. 

### Environment:
```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-6850K CPU 3.60GHz (Skylake), 1 CPU, 12 logical and 6 physical cores
64 GB DDR4 Memory
.NET SDK=5.0.402
  [Host]     : .NET 5.0.11 (5.0.1121.47308), X64 RyuJIT
  Job-YXUPLH : .NET 5.0.11 (5.0.1121.47308), X64 RyuJIT
```

### Benchmark 1 (1M for loop)
Simple For:
``` javascript
f1 = (i) => i * i

for (var i = 0.0 ; i < 1000000; ++i) {
    f1(i)
}
```

Host For (Single-Thread):
``` javascript
Host.For(0, 1000000 , (i) => i + i)
```

Parallel For:
``` javascript
Parallel.For(0, 1000000 , (i) => i + i)
```

Result:
``` ini
|              Method |         Mean |      Error |     StdDev |
|-------------------- |-------------:|-----------:|-----------:|
| Topaz Parallel For  |   123.895 ms |  2.4524 ms |  3.3568 ms |
| Topaz Host For      |   236.638 ms |  4.0474 ms |  4.4987 ms |
| Topaz Simple For    |   513.773 ms |  3.1686 ms |  2.9640 ms |
| V8Engine Parallel   |                          NOT SUPPORTED |
| V8Engine Simple For |     7.350 ms |  0.1438 ms |  0.1969 ms |
| V8Engine Host For   |  8235.497 ms | 63.0376 ms | 55.8812 ms |
| Jint Parallel For   |                          NOT SUPPORTED |
| Jint Host For       |   355.578 ms |  2.9865 ms |  2.6475 ms |
| Jint Simple For     |   861.403 ms |  9.4439 ms |  8.8338 ms |
```
Comments:

ClearScript V8 Engine is super optimized for isolated scripts that does not communicate with the host.
However, it is extremely slow (1M Loop in 8 seconds!) if the script code interacts with host.
Topaz is significantly faster than Jint in simple for loops. Moreover, Topaz can iterate loops in parallel using .NET Task Parallel Library (TPL).

### Benchmark 2 (1M manipulations on arbitrary .NET objects)
Code:
``` javascript
for (var i = 0.0 ; i < 1000000; ++i) {
    model.Value++;
}
```
Result:
``` ini
|      Method |        Mean |   Error |  StdDev |
|------------ |------------:|--------:|--------:|
|    RunTopaz |    792.0 ms | 9.74 ms | 9.11 ms |
|     RunJint |    896.6 ms | 7.78 ms | 7.28 ms |
| RunV8Engine | 30,598.6 ms |    NA   |    NA   |
```

Comments:

V8 engine is extremely slow because the script code interacts with host object.
Topaz is slightly faster than Jint with manipulations on arbitrary .NET object.

### Benchmark 3 (1M manipulation on Dictionary<,>)
Code:
``` javascript
for (var i = 0.0 ; i < 1000000; ++i) {
    model.Value++;
}
```
Result:
``` ini
|      Method |       Mean |    Error |   StdDev |
|------------ |-----------:|---------:|---------:|
|    RunTopaz |   784.4 ms | 15.06 ms | 14.79 ms |
|     RunJint | 1,449.0 ms |  6.01 ms |  4.69 ms |
| RunV8Engine |   Error    |    NA    |    NA    |
```

Comments:

Topaz is significantly faster than Jint in this benchmark. The model object is a Dictionary<string, int>.

### Benchmark 4 (1M arbitrary .NET object method call)
Code:
``` javascript
for (var i = 0.0 ; i < 1000000; ++i) {
    model.Increment();
}
```
Result:
``` ini
|      Method |     Mean   |    Error |   StdDev |
|------------ |-----------:|---------:|---------:|
|    RunTopaz |   640.4 ms |  3.93 ms |  3.68 ms |
|     RunJint |   878.7 ms | 12.15 ms | 11.37 ms |
| RunV8Engine | 8180.00 ms | 4.691 ms | 4.388 ms |
```

Comments:

Topaz is significantly faster than Jint in this benchmark. The model object is a .NET class. This benchmark demonstrates .NET function calls from script.

### Benchmark 5 (100K arbitrary .NET objects manipulation and addition into an array)
Code:
``` javascript
var list = []
for (const item of model.Profiles) {
    let x = {}
    x.Name = item.Name + 1
    x.Address = item.Address + 1
    x.Bio = item.Bio + 1
    x.City = item.City + 1
    x.Country = item.Country + 1
    x.Email = item.Email + 1
    x.Phone = item.Phone + 1
    list.push(x)
}
model.List = list
```
Result:
``` ini
|      Method |      Mean |    Error |   StdDev |
|------------ |----------:|---------:|---------:|
|    RunTopaz |  1.038 s  | 0.0141 s | 0.0125 s |
|     RunJint |  1.465 s  | 0.0194 s | 0.0182 s |
| RunV8Engine | 33,3009 s |  0.173 s |  0.512 s |
```

Comments:

Topaz is significantly faster than Jint in this benchmark. The model object is a .NET class. This benchmark demonstrates 100K object iteration and creation with several properties. Please note that using Topaz JsObject or an object that implements IDictionary instead of an arbitrary class would avoid reflection and boost the performance of your script. This is not added as a separate benchmark as it would not be fair to compare that with other engines.

### Benchmark 6 (1M Javascript parallel function call from Host)
Code:
``` javascript
function square(x) {
    return x*x;
}
```
``` c#
// calls script function from .NET
Parallel.For(0, LoopLength, (x) =>
{
    var result = engine.Invoke("square", x); ;
});
```
Result:
``` ini
|      Method |        Mean |      Error |    StdDev |
|------------ |------------:|-----------:|----------:|
|    RunTopaz |     237.9 ms |   4.87 ms |  14.35 ms |
|     RunJint |     745.7 ms |   3.52 ms |   3.30 ms |
| RunV8Engine | 13,630.36 ms | 14.993 ms | 14.025 ms |
```

Comments:

Topaz is significantly faster than Jint on this benchmark. Jint functions are not thread-safe even when they do not manipulate any shared object.
Topaz functions can be safely called from multiple threads in parallel.

## Story of Topaz
I have developed a server-side HTML page rendering application using Razor. After a while, I have realized Razor was not flexible for my microservice architecture and business needs.

Then I switched to [ClearScript](https://github.com/microsoft/ClearScript). Clearscript uses Google's v8 C++ engine using locks and marshaling everywhere which slows down things. I barely served 100 requests per second on my desktop. Moreover, memory consumption was too high: 800 MB for a couple of page rendering.

Then I switched to [Jint](https://github.com/sebastienros/jint). Jint is very successful in terms of performance (I get ~1600 page views per second) and memory consumption is also good. (~80 MB)
Jint is excellent with its extreme support for Javascript native data structures.
The only downside of Jint is it does not support parallel execution on even simple function calls. I had to lock my page request. That was not acceptable for my business.

Thanks to Sebastien Ros. He has written an excellent [Esprima](https://github.com/sebastienros/esprima-dotnet) port which makes writing JS runtime a piece of cake.

So I decided to write my own!

This is how Topaz was born.

I hear you are saying tell me the numbers.

Here is the answer:

Topaz Performance on server-side rendering hits 3700 per second. Memory consumption is ~80MB. It is a good start. Topaz performs better than Jint in performance. Moreover, it supports multi-threading out of the box!
The server-side rendering application that I wrote using Topaz surpasses Razor runtime rendering with x2 faster execution.
Additionally, Razor initialization takes a few seconds that is annoying. Topaz is ready at first glance!

That is just the beginning. There is still room for performance improvements. Furthermore, a lot of features in the backlog are waiting for implementation.

Stay in touch.

## How can I use Topaz?

Here is the [Nuget Link](https://www.nuget.org/packages/Topaz/).

### Hello world application:
```c#
var engine = new TopazEngine();
engine.AddType(typeof(Console), "Console");
engine.SetValue("name", "Topaz");
engine.ExecuteScript(@"Console.WriteLine('Hello World, from ' + name)");
```

### Async Http Get:
An example of fetching HTTP content without blocking executing thread.
```c#
var engine = new TopazEngine();
engine.AddType<HttpClient>("HttpClient");
engine.AddType(typeof(Console), "Console");
var task = engine.ExecuteScriptAsync(@"
async function httpGet(url) {
    try {
        var httpClient = new HttpClient()
        var response = await httpClient.GetAsync(url)
        return await response.Content.ReadAsStringAsync()
    }
    catch (err) {
        Console.WriteLine('Caught Error:\n' + err)
    }
    finally {
        httpClient.Dispose();
    }
}
const html = await httpGet('http://example.com')
Console.WriteLine(html);
");
task.Wait();

```

### Parallel For loop:
An example of parallel for loop. Notice read/write global shared variable simultaneously with no crash!
```c#
var engine = new TopazEngine();
engine.AddType(typeof(Console), "Console");
topazEngine.AddType(typeof(Parallel), "Parallel");
engine.ExecuteScript(@"
var sharedVariable = 0
function f1(i) {
    sharedVariable = i
}
Parallel.For(0, 100000 , f1)
Console.WriteLine(`Final value: {sharedVariable}`);
");

```

### Generic Constructors:
An example of generic type construction. Generic Type Arguments are passed through constructor function arguments.
```c#
var engine = new TopazEngine();
var types = new JsObject();
engine.SetValue("Types", types);
types["int"] = typeof(int);
types["string"] = typeof(string);
engine.AddType(typeof(Dictionary<,>), "Dictionary");
engine.AddType(typeof(Console), "Console");
engine.ExecuteScript(@"
var dic = new Dictionary(Types.string, Types.int)
dic.Add('hello', 1)
dic.Add('dummy', 0)
dic.Add('world', 2)
dic.Remove('dummy')
Console.WriteLine(`Final value: {dic['hello']} {dic['world']}`);
");

```

### Cancellation support:
Cancellation of a long running script with time constraint.
```c#
try
{
  var engine = new TopazEngine();
  using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
  engine.ExecuteScript(@"
var i = 0;
while(true) {
  ++i; // infinite loop
}", cancellationSource.Token);
}
catch (OperationCanceledException e) {
    // ...
}
```

### Linq and Extension method support:
You can use Linq or other extension methods in your script. Underlying argument type conversion and method selection is automated.
```c#
var engine = new TopazEngine();
engine.AddExtensionMethods(typeof(Enumerable));
engine.AddType(typeof(Console), "Console");
var items = Enumerable.Range(1, 100).Select(x => new
{
    Name = "item " + x,
    Index = x
}).ToArray();
engine.SetValue("items", items);
engine.ExecuteScript(@"
var filteredItems =
    items
    .Where((x) => x.Index % 2 == 1)
    .Select((x, i) => x.Name + ' : ' + i)
    .ToArray()

for (var item of filteredItems)
{
    Console.WriteLine(item.Name)
}
}");
```

### Built-in Javascript Objects:
The global scope of Topaz is empty by default. You can add objects from `Tenray.Topaz.API` namespace which contains Javascript well known objects.
```c#
var engine = new TopazEngine();
engine.SetValue("JSON", new JSONObject());
engine.SetValue("globalThis", new GlobalThis(engine.GlobalScope));
engine.AddType(typeof(Console), "Console");
engine.ExecuteScript(@"
var items = [1,2,3]
var json = JSON.stringify(a)
Console.WriteLine(json);
var parsedArray = globalThis.JSON.parse(json);
");
```

### Exposing namespaces to the script:
Exposing namespaces is supported with secure options.
It is strongly recommended to provide a whitelist to the AddNamespace method.
If you pass null whitelist, every type in the given namespace will be available.
This is dangerous for System namespaces and many other. For example System.Type, System.AppDomain and much more.
The only secure way to allow namespaces is to give a whitelist.
You can omit whitelist for your custom namespaces with caution.
You can allow sub namespaces with the 3rd parameter. See the method comments for further information.

```c#
var engine = new TopazEngine();
var whitelist = new HashSet<string> {
    "System.Int32",
    "System.String",
    "System.Collections.Generic.Dictionary"
};
engine.AddNamespace("System", whitelist, true);
engine.ExecuteScript(@"
var dictionary = new System.Collections.Generic.Dictionary(System.String, System.Int32)
dictionary.Add('key1', 13)
");
```


### Generic method arguments selection:
In C# you can select which types should be used in generic method execution.
Javascript does not have generic methods. To overcome this problem, there is a special method defined in Topaz called `GenericArguments`.
This method accepts type parameters. You can use the method when you want to call a generic method with explicit type information.

If you don't provide generic type arguments by calling `GenericArguments`, all generic arguments are selected as `object` by default.
For extension methods, the first generic argument type can be deducted from this parameter when `GenericArguments` is not used.

`GenericArguments` only affects subsequent method call.

``` c#
var engine = new TopazEngine();
engine.AddNamespace("System");
engine.AddExtensionMethods(typeof(Enumerable));
var items = Enumerable.Range(1, 100).Select(x => new
{
    Name = "item " + x,
    Index = x
}).ToArray();
engine.SetValue("items", items);
engine.ExecuteScript(@"
var a = items.ToDictionary(x => x.Index, y => y.Name) // produces Dictionary<object, object>

var b = items
    .GenericArguments(System.Object, System.Int32, System.String)
    .ToDictionary(x => x.Index, y => y.Name) // produces Dictionary<int, string>

var c = items
    .GenericArguments(items[0].GetType(), System.Double, System.String)
    .ToDictionary(x => x.Index, y => y.Name) // produces Dictionary<double, string>
    
var d = items.ToDictionary(x => x.Index) // produces Dictionary<object, AnonymousType> (type deduction from this parameter)
");
```

### Fully Customizable Type Conversions:
Topaz provides great interop capabilities with automatic type conversions. If you need something special for specific types or you want a full replacement, you can use following interfaces.
Every type conversion operation is customizable.
```c#
public interface ITypeProxy
{
    /// <summary>
    /// Proxied type.
    /// </summary>
    Type ProxiedType { get; }
    /// <summary>
    ///  Calls generic or non-generic constructor.
    /// </summary>
    /// <param name="args">Generic and constructor arguments.</aram>
    /// <returns></returns>
    object CallConstructor(IReadOnlyList<object> args);
    /// <summary>
    /// Returns true if a member is found, false otherwise.
    /// Output value is member of the static type.
    /// If static type member is a function then output value is Invokable.
    /// </summary>
    /// <param name="member">Member name or indexed member value.</aram>
    /// <param name="value">Returned value.</param>
    /// <param name="isIndexedProperty">Indicates if the member
    /// retrieval should be through an indexed property.</param>
    /// <returns></returns>
    bool TryGetStaticMember(
        object member,
        out object value,
        bool isIndexedProperty = false);
    /// <summary>
    /// Returns true if a member is found and updated with new alue,
    /// false otherwise.
    /// If statyic type member is a function, returns false.
    /// </summary>
    /// <param name="member">Member name or indexed member value.</aram>
    /// <param name="value">New value.</param>
    /// <param name="isIndexedProperty">Indicates if the member
    /// retrieval should be through an indexed property.</param>
    /// <returns></returns>
    bool TrySetStaticMember(
        object member,
        object value,
        bool isIndexedProperty = false);
}
public interface IObjectProxy
...
public interface IInvokable
...
public interface IDelegateInvoker
...
public interface IObjectProxyRegistry
...
public interface IValueConverter
...

```

### Security:

`SecurityPolicy` property of `ITopazEngine.Options` provides high level security aspect of the script runtime.
Default security policy does not allow Reflection API calls in script runtime.
Enable Reflection Policy will let script author to access everything through Reflection.
eg:
```javascript
someValue
    .GetType()
    .Assembly
    .GetType('System.IO.File')
// or
someValue
    .GetType()
    .GetProperty('Some Private Property Name')
    .GetValue(someValue)
```
Enabling Reflection is not recommended.

##### Available security policy flags:
```c#
public enum SecurityPolicy
{
    /// <summary>
    /// Default and secure. Script cannot process
    /// types in the System.Reflection namespace.
    /// </summary>
    Default,
    /// <summary>
    /// Reflection API is allowed.
    /// Script can access everything.
    /// Use it with caution.
    /// </summary>
    EnableReflection
}
```
##### Object member access policy:
Object member access security policy enables individual control on object member access.
The default implementation is a whitelist for members of `Type` class
and allows everything for any other type.
`Type` whitelist is important to prevent unexpected private member access.

##### Custom object member access behavior:
You can pass a custom MemberAccessPolicy implementation to Topaz Engine
via constructor.

`ITopazEngine.MemberAccessPolicy` interface defines a method to control custom member access behavior in runtime.

It is recommended to call DefaultMemberAccessPolicy in your custom `IMemberAccessPolicy` definition.

Otherwise you may not get benefit of covered security leaks by Topaz.

### Feature List:
* for loops
* for .. in iterators
* for .. of iterators
* switch case statement
* functions
* arrow functions
* object destructring
* array destructring
* await
* if else statements
* while, do while statements
* conditional statements
* rest and spread elements (...)
* template literals
* tagged template literals
* try catch finally statement
* throw statement
* new expression (constructor)
* static member and function call of CLR types
* automatic type conversion for CLR calls
* binary operators
* unary operators
* flow statements (break, continue, return)
* optional chaining
* typeof operator
* instanceof operator
* in operator
* top level await statement
* globalThis

more is coming...

Despite the fact that the current feature set is more than enough for my needs, I am eager to improve this engine to support a lot more.

I appreciate any feedback and contributions to the project.

## Topaz Engine Options:

```c#
  SecurityPolicy: Default | EnableReflection

  VarScopeBehavior: FunctionScope | DeclarationScope

  AssignmentWithoutDefinitionBehavior:
    DefineAsVarInExecutionScope |
    DefineAsVarInGlobalScope |
    DefineAsVarInFirstChildOfGlobalScope |
    DefineAsLetInExecutionScope |
    ThrowException

  NoUndefined: true | false

  AllowNullReferenceMemberAccess: true | false

  AllowUndefinedReferenceMemberAccess: true | false

  AllowUndefinedReferenceAccess: true | false

  LiteralNumbersAreConvertedToDouble: true | false

  NumbersAreConvertedToDoubleInArithmeticOperations:  true | false

  UseThreadSafeJsObjects: true | false
```

## Notes on arithmetic operations and literal types:

C# is a type-safe language but Javascript is not.
Topaz encapsulates the differences by using auto type conversions.

##### Literal Numbers:
If you want explicit behavior for literal number evaluation, you can use `LiteralNumbersAreConvertedToDouble` option.

If `LiteralNumbersAreConvertedToDouble` option is true, literal numbers that are written in script are converted to double,

if `LiteralNumbersAreConvertedToDouble` option is false the type is deducted by string itself.

Deducted type can be int, long or double.
For other types use type conversion functions in your script.
For example: 3 => int, 2147483648 => long, 3.2 => double

Default `LiteralNumbersAreConvertedToDouble` option value is false.

##### Arithmetic Operations:
If `NumbersAreConvertedToDoubleInArithmeticOperations` option is true, arithmetic operations will change numeric value types to double to avoid overflows.

If `NumbersAreConvertedToDoubleInArithmeticOperations` option is false, the types stay same only if no overflow happens.

If overflow happens, types are converted in the following order:
int -> long -> double

That operation is slow because of thrown overflow exception.
Especially, if so many overflow exceptions occurs in a loop, execution duration increases dramatically.

Default `NumbersAreConvertedToDoubleInArithmeticOperations` option value is true.

If `NumbersAreConvertedToDoubleInArithmeticOperations` option is set to false, binary/unary operations do automatic type conversion in case of overflow using checked statements which is slow when it overflows.

Hence, it is a good decision to convert everything to double to avoid unexpected slow overflow exceptions in binary/unary operations unless you need exact integer arithmetics for non-floating values.

If you don't want to automatically change value types in arithmetic operations, disable this option and keep in mind that your script should care about slowness if there are too many overflows.

On the other hand, operations on int, long are faster than double.
If you want to explicitly handle number types in the script runtime
you may choose false.

Both options have pros and cons, choose wisely.

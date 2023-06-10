![img](https://raw.githubusercontent.com/koculu/Topaz/main/src/Topaz/docs/Topaz/images/logo1.png)

# Topaz
Topaz is a high-performance, multithreaded JavaScript engine for the .NET platform. It is designed to overcome the limitations of existing JavaScript engines by providing robust multithreading support and syntax-level options that simplify script language usage. With Topaz, you can seamlessly execute JavaScript scripts on the server-side with blazing speed and harness the full power of the .NET runtime.

[![Downloads](https://img.shields.io/nuget/dt/Topaz)](https://www.nuget.org/packages/Topaz/)

# Why another Javascript Engine?

While existing JavaScript engines adequately handle script execution in single-threaded desktop environments, they often fall short when it comes to server-side execution. Topaz addresses this limitation by allowing for concurrent multithreaded usage, enabling efficient execution of JavaScript code in server applications.

To gain a deeper understanding of the capabilities and versatility of Topaz, I encourage you to check out the [TopazView](https://github.com/koculu/TopazView) project.

# Syntax-Level Options

Topaz offers syntax-level options that streamline the scripting support for various products. These options provide flexibility in managing variables, such as controlling assignment scope and disabling null reference exceptions, without disrupting the script's flow. By incorporating these options, Topaz ensures a more intuitive and convenient scripting experience.

# Lock-Free Scripting Engine

One of the key goals of Topaz is to provide a lock-free scripting engine. By minimizing locking mechanisms, Topaz optimizes performance and eliminates potential bottlenecks, resulting in faster and more efficient script execution. This design choice makes Topaz an ideal choice for high-performance server applications that require concurrent execution of JavaScript code.

# How Does Topaz Work?

Unlike traditional JavaScript engines like V8, which convert JavaScript code directly into machine code, Topaz takes a different approach. It parses the abstract syntax tree of the JavaScript code and executes instructions within the .NET runtime environment. By leveraging the power of the .NET runtime, Topaz achieves a seamless integration between JavaScript and .NET, enabling JavaScript variables to be treated as native .NET types.

While Topaz may not support the entire spectrum of JavaScript runtime features, it serves as a lightweight script interpreter that harnesses the full capabilities of the .NET runtime. This approach ensures compatibility with existing .NET libraries and provides access to the vast ecosystem of tools and functionalities available in the .NET ecosystem.

# Key Features
* Multithreaded JavaScript execution for enhanced performance in server-side applications.
* Syntax-level options that simplify script language usage, providing greater flexibility and control.
* Lock-free scripting engine for optimal performance in concurrent execution scenarios.
* Seamless integration with the .NET runtime, allowing JavaScript variables to be treated as native .NET types.

# Getting Started
Getting started with Topaz is quick and easy. Simply install the Topaz NuGet package and start leveraging the power of the multithreaded JavaScript engine for your .NET projects.

Join our vibrant community of developers and contribute to the project's growth by reporting issues, suggesting improvements, or submitting pull requests.

# License
Topaz is released under the MIT License. Feel free to use, modify, and distribute the project according to the terms of the license.

# The performance of Topaz

Topaz is designed to deliver excellent performance in JavaScript execution, surpassing that of other engines such as ClearScript and Jint. While we have focused on optimizing Topaz for a wide range of scenarios, it's worth noting that performance can vary based on specific use cases.

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
| Topaz Parallel For  |   128.361 ms |  1.5664 ms |  1.4652 ms |
| Topaz Host For      |   225.279 ms |  1.2184 ms |  0.9513 ms |
| Topaz Simple For    |   465.666 ms |  3.4883 ms |  3.2630 ms |
| V8Engine Parallel   |                          NOT SUPPORTED |
| V8Engine Simple For |     6.006 ms |  0.1129 ms |  0.1108 ms |
| V8Engine Host For   | 2,647.886 ms | 32.2750 ms | 30.1901 ms |
| Jint Parallel For   |                          NOT SUPPORTED |
| Jint Host For       |   339.026 ms |  3.6222 ms |  3.2110 ms |
| Jint Simple For     |   627.941 ms | 11.4362 ms | 10.1379 ms |
```
Comments:

ClearScript V8 Engine is super optimized for isolated scripts that don't communicate with the host.
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
|      Method |        Mean |   Error  |  StdDev |
|------------ |------------:|---------:|--------:|
|    RunTopaz | 548.9 ms    | 10.42 ms | 9.23 ms |
|     RunJint | 616.7 ms    |  4.62 ms | 4.10 ms |
| RunV8Engine | 30,598.6 ms |    NA    |    NA   |
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
|    RunTopaz | 620.6 ms   | 11.61 ms | 10.86 ms |
|     RunJint | 730.9 ms   |  4.31 ms |  4.03 ms |
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
|    RunTopaz |   401.4 ms | 2.35 ms  |  1.83 ms |
|     RunJint |   448.6 ms | 1.07 ms  |  0.90 ms |
| RunV8Engine | 8180.00 ms | 4.691 ms | 4.388 ms |
```

Comments:

Topaz is slightly faster than Jint in this benchmark. The model object is a .NET class. This benchmark demonstrates .NET function calls from script.

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
|      Method |      Mean  |    Error |   StdDev |
|------------ |-----------:|---------:|---------:|
|    RunTopaz |  866.2 ms  | 200.4 ms | 132.6 ms |
|     RunJint | 2,708.2 ms | 692.0 ms | 457.7 ms |
| RunV8Engine | 33,3009 s  |  0.173 s |  0.512 s |
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
|    RunTopaz |   224.1 ms  |    4.39 ms |   4.70 ms |
|     RunJint |   653.1 ms  |    4.64 ms |   4.12 ms |
| RunV8Engine | 13,630.36 ms | 14.993 ms | 14.025 ms |
```

Comments:

Topaz is significantly faster than Jint on this benchmark. Jint functions are not thread-safe even when they do not manipulate any shared object.
Topaz functions can be safely called from multiple threads in parallel.

## The Evolution of Topaz

The story of Topaz is one of continuous improvement and the pursuit of excellence. It all started when I developed a server-side HTML page rendering application using Razor. However, as my microservice architecture and business needs evolved, I realized that Razor wasn't as flexible as I had hoped.

In my quest for a more optimized solution, I turned to ClearScript. While ClearScript relied on Google's v8 C++ engine, it suffered from performance issues due to locks and marshaling, resulting in sluggish response times. Even on my desktop, I could barely serve 100 requests per second, and memory consumption soared to 800 MB for just a few page renderings.

Seeking a better alternative, I discovered Jint. Jint proved to be highly performant, boasting approximately 1600 page views per second, with a modest memory footprint of around 80 MB. Jint's remarkable support for JavaScript native data structures was impressive. However, its lack of parallel execution for even simple function calls meant I had to lock my page requests, which proved unacceptable for my business requirements.

Fortunately, I came across the remarkable work of Sebastien Ros, who had created an excellent Esprima port. This discovery inspired me to embark on a journey of creating my own solution.

And so, Topaz was born.

I can hear you asking for the numbers, so here they are:

With Topaz, server-side rendering performance soars to an impressive 3700 requests per second, while memory consumption remains at approximately 80 MB. These initial figures demonstrate Topaz's superiority over Jint in terms of performance. What's more, Topaz boasts built-in support for multi-threading, eliminating the need for manual locking. The server-side rendering application I developed using Topaz surpasses Razor's runtime rendering with twice the speed of execution. Gone are the few seconds of annoying Razor initialization. With Topaz, everything is ready at first glance.

But this is only the beginning. There is still ample room for further performance improvements, and a plethora of exciting features in the backlog eagerly await implementation.

Stay connected and witness the evolution of Topaz.


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
* object destructuring
* array destructuring
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

## Acknowledgments

We would like to express our gratitude to the open-source community for their invaluable contributions and support in making Topaz a powerful and efficient JavaScript engine for the .NET platform. We also extend our thanks to the developers of existing JavaScript engines and wrappers for inspiring our work.
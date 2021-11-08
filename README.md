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
var model = new CaseSensitiveDynamicObject();
engine.SetValue("model", model);
model["int"] = typeof(int);
model["string"] = typeof(string);
engine.AddType(typeof(Dictionary<,>), "GenericDictionary");
engine.AddType(typeof(Console), "Console");
engine.ExecuteScript(@"
var dic = new GenericDictionary(model.string, model.int)
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
} catch (OperationCanceledException e) {
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
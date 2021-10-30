# Topaz
Multithreaded Javascript Engine for .NET

## Why another Javascript Engine?

1. Existing Javascript Engines run scripts in single-thread. That is acceptable for desktop environments but not pleasing for server-side execution.

2. I needed fine control in the language syntax and rules for my project.
I could not find any engine that provides such features.
For example, I want to turn off 'undefined' and treat all as 'null', or I want 'assignments without variable kind' scope to be in declaration scope but not in the global scope.

3. Another important need is to turn off every null pointer exception including function call or member access of a 'null' object. These will ease the life of non-experienced developers to write scripts without putting everywhere question marks (condition operator, null checks) or getting annoying errors.

4. I want minimum amount of overhead, no C++ marshaling and minimum memory usage.

5. Lock free and high-performance implementation.

## How is the performance of Topaz?
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
        /// <param name="args">Generic and constructor arguments.</param>
        /// <returns></returns>
        object CallConstructor(IReadOnlyList<object> args);

        /// <summary>
        /// Returns true if a member is found, false otherwise.
        /// Output value is member of the static type.
        /// If static type member is a function then output value is IInvokable.
        /// </summary>
        /// <param name="member">Member name or indexed member value.</param>
        /// <param name="value">Returned value.</param>
        /// <param name="isIndexedProperty">Indicates if the member
        /// retrieval should be through an indexed property.</param>
        /// <returns></returns>
        bool TryGetStaticMember(
            object member,
            out object value,
            bool isIndexedProperty = false);

        /// <summary>
        /// Returns true if a member is found and updated with new value,
        /// false otherwise.
        /// If statyic type member is a function, returns false.
        /// </summary>
        /// <param name="member">Member name or indexed member value.</param>
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

more is coming...

Despite the fact that the current feature set is more than enough for my needs, I am eager to improve this engine to support a lot more.

I appreciate any feedback and contributions to the project.

## Topaz Engine Options:

```c#
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
```

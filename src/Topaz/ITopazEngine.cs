using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Interop;
using Tenray.Topaz.Options;

namespace Tenray.Topaz;

public interface ITopazEngine
{
    /// <summary>
    /// Engine unique id.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// True if engine is thread-safe, false otherwise.
    /// </summary>
    bool IsThreadSafe { get; }

    /// <summary>
    /// Engine Options.
    /// </summary>
    TopazEngineOptions Options { get; set; }

    /// <summary>
    /// Global Scope.
    /// </summary>
    ITopazEngineScope GlobalScope { get; }

    /// <summary>
    /// Object Proxy Registry.
    /// </summary>
    IObjectProxyRegistry ObjectProxyRegistry { get; }

    /// <summary>
    /// Default Object Proxy.
    /// </summary>
    IObjectProxy DefaultObjectProxy { get; }

    /// <summary>
    /// Delegate invoker.
    /// Invokes Action<>, Func<,..> or any delegate.
    /// </summary>
    IDelegateInvoker DelegateInvoker { get; }

    /// <summary>
    /// Object member access policy handler.
    /// If you override this, make sure to call 
    /// DefaultMemberAccessPolicy methods in your
    /// custom class.
    /// </summary>
    IMemberAccessPolicy MemberAccessPolicy { get; }

    /// <summary>
    /// Value Converter is an interface that does value conversions in assignments.
    /// </summary>
    IValueConverter ValueConverter { get; }

    /// <summary>
    /// Executes the script in the global scope.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="token"></param>
    void ExecuteScript(string code, CancellationToken token = default);

    /// <summary>
    /// Executes the single line expression in the global scope.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    object ExecuteExpression(string code, CancellationToken token = default);

    /// <summary>
    /// Executes the script in the global scope.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="token"></param>
    Task ExecuteScriptAsync(string code, CancellationToken token = default);

    /// <summary>
    /// Executes the single line expression in the global scope.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<object> ExecuteExpressionAsync(string code, CancellationToken token = default);

    /// <summary>
    /// Executes the function that is declared in the global scope with given name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="token"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    object InvokeFunction(string name, CancellationToken token, params object[] args);

    /// <summary>
    /// Executes given function object.
    /// </summary>
    /// <param name="functionObject">A Javascript function, Action<> or Func<> object.</param>
    /// <param name="token"></param>
    /// /// <param name="args">The arguments passed into the function.</param>
    /// <returns></returns>
    object InvokeFunction(object functionObject, CancellationToken token, params object[] args);

    /// <summary>
    /// Executes the function that is declared in the global scope with given name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="token"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<object> InvokeFunctionAsync(string name, CancellationToken token, params object[] args);

    /// <summary>
    /// Executes given function object.
    /// </summary>
    /// <param name="functionObject">A Javascript function, Action<> or Func<> object.</param>
    /// <param name="token"></param>
    /// <param name="args">The arguments passed into the function.</param>
    /// <returns></returns>
    Task<object> InvokeFunctionAsync(object functionObject, CancellationToken token, params object[] args);

    /// <summary>
    /// Adds a type to be used in Javascript.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="typeProxy"></param>
    void AddType<T>(string name = null, ITypeProxy typeProxy = null);

    /// <summary>
    /// Adds a type to be used in Javascript.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="typeProxy"></param>
    void AddType(Type type, string name = null, ITypeProxy typeProxy = null);

    /// <summary>
    /// Adds extension methods defined in type.
    /// </summary>
    /// <param name="type">The type that defines extension methods.</param>
    public void AddExtensionMethods(Type type);

    /// <summary>
    /// Adds given namespace to the script. It is dangerous to enable entire namespace in the script environment.
    /// There might be types like System.AppDomain, System.Type which gives exclusive access to your process.
    /// To overcome the security leak, AddNamespace function accepts a whitelist.
    /// If you omit the whitelist, script will get access to everything in given namespace.
    /// Use it with caution !
    /// </summary>
    /// <param name="namespace">The full name of the namespace.</param>
    /// <param name="whitelist">The whitelist contains the types that are allowed.</param>
    /// <param name="allowSubNamespaces">If true, subnamespaces will be accessible.</param>
    /// <param name="name">The name that will be used in script to access given namespace.
    /// If this is not provided the script name will be equal to the namespace.</param>
    public void AddNamespace(string @namespace, ISet<string> whitelist, bool allowSubNamespaces = false, string name = null);

    /// <summary>
    /// Gets the value of the variable that is defined in the global scope.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <returns></returns>
    object GetValue(string name);

    /// <summary>
    /// Defines a new variable with given name and value
    /// or updates existing variable with given name and value
    /// in the global scope.
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <param name="value">The value</param>
    void SetValue(string name, object value);

    /// <summary>
    /// Defines a new variable with given name, value and kind
    /// or updates existing variable with given name, value and kind
    /// in the global scope.
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <param name="value">The value</param>
    /// <param name="variableKind">Variable kind, var, let or const.</param>
    void SetValueAndKind(string name, object value, VariableKind variableKind);
}

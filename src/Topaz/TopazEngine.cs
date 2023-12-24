using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Interop;
using Tenray.Topaz.Options;

namespace Tenray.Topaz;

public sealed class TopazEngine : ITopazEngine
{
    private static int lastTopazEngineId;

    private readonly ScriptExecutor globalScope;

    private readonly ExtensionMethodRegistry extensionMethodRegistry;

    public int Id { get; }

    public bool IsThreadSafe => GlobalScope.IsThreadSafe;

    public TopazEngineOptions Options { get; set; }

    public ITopazEngineScope GlobalScope => globalScope;

    public IObjectProxyRegistry ObjectProxyRegistry { get; }

    public IObjectProxy DefaultObjectProxy { get; }

    public IDelegateInvoker DelegateInvoker { get; }

    public IMemberAccessPolicy MemberAccessPolicy { get; }

    public IValueConverter ValueConverter { get; }

    public IMemberInfoProvider MemberInfoProvider { get; }

    internal ScriptExecutorPool ScriptExecutorPool = new();

    public TopazEngine(TopazEngineSetup setup = null)
    {
        if (setup == null)
            setup = new TopazEngineSetup();
        Id = Interlocked.Increment(ref lastTopazEngineId);
        globalScope = new ScriptExecutor(this, setup.IsThreadSafe);
        var optionsHasGiven = setup.Options != null;
        Options = setup.Options ?? PresetOptions.FriendlyStyle;
        if (!optionsHasGiven)
            Options.UseThreadSafeJsObjects = setup.IsThreadSafe;
        ObjectProxyRegistry = setup.ObjectProxyRegistry ?? new DictionaryObjectProxyRegistry();
        extensionMethodRegistry = new();
        MemberInfoProvider = setup.MemberInfoProvider ?? new MemberInfoProvider();
        ValueConverter = setup.ValueConverter ?? new DefaultValueConverter();
        DefaultObjectProxy = setup.DefaultObjectProxy ?? new ObjectProxyUsingReflection(null, extensionMethodRegistry, ValueConverter, MemberInfoProvider);
        DelegateInvoker = setup.DelegateInvoker ?? new DelegateInvoker(ValueConverter);
        MemberAccessPolicy = setup.MemberAccessPolicy ?? new DefaultMemberAccessPolicy(this);
    }

    public void ExecuteScript(string code, CancellationToken token = default)
    {
        GlobalScope.ExecuteScript(code, token);
    }

    public object ExecuteExpression(string code, CancellationToken token = default)
    {
        return GlobalScope.ExecuteExpression(code, token);
    }

    public object InvokeFunction(string name, CancellationToken token, params object[] args)
    {
        return GlobalScope.InvokeFunction(name, token, args);
    }

    public object InvokeFunction(object functionObject, CancellationToken token, params object[] args)
    {
        return GlobalScope.InvokeFunction(functionObject, token, args);
    }

    public void AddType<T>(string name = null, ITypeProxy typeProxy = null)
    {
        AddType(typeof(T), name, typeProxy);
    }

    public void AddType(Type type, string name = null, ITypeProxy typeProxy = null)
    {
        GlobalScope.SetValueAndKind(
            name ?? type.FullName,
            typeProxy ??
            TypeProxyUsingReflection.GetTypeProxy(type) ??
            new TypeProxyUsingReflection(type, ValueConverter, MemberInfoProvider, name),
            VariableKind.Const);
    }

    public void AddExtensionMethods(Type type)
    {
        extensionMethodRegistry.AddType(type);
    }

    public void AddNamespace(string @namespace, ISet<string> whitelist = null, bool allowSubNamespaces = false, string name = null)
    {
        GlobalScope.SetValueAndKind(
            name ?? @namespace,
            new NamespaceProxy(@namespace, whitelist, allowSubNamespaces, ValueConverter, MemberInfoProvider),
            VariableKind.Const);
    }

    public object GetValue(string name)
    {
        return GlobalScope.GetValue(name);
    }

    public void SetValue(string name, object value)
    {
        GlobalScope.SetValue(name, value);
    }

    public void SetValueAndKind(string name, object value, VariableKind variableKind)
    {
        GlobalScope.SetValueAndKind(name, value, variableKind);
    }

    public async Task ExecuteScriptAsync(string code, CancellationToken token = default)
    {
        await GlobalScope.ExecuteScriptAsync(code, token);
    }

    public async Task<object> ExecuteExpressionAsync(string code, CancellationToken token = default)
    {
        return await GlobalScope.ExecuteExpressionAsync(code, token);
    }

    public async Task<object> InvokeFunctionAsync(string name, CancellationToken token, params object[] args)
    {
        return await GlobalScope.InvokeFunctionAsync(name, token, args);
    }

    public async Task<object> InvokeFunctionAsync(object functionObject, CancellationToken token, params object[] args)
    {
        return await GlobalScope.InvokeFunctionAsync(functionObject, token, args);
    }

    internal bool TryGetObjectMember(
        object instance,
        object member,
        out object value,
        bool isIndexedProperty = false)
    {
        if (instance == null)
        {
            value = Options.NoUndefined ? null : Undefined.Value;
            return false;
        }

        ProcessObjectMemberSecurityPolicy(instance, member);

        if (instance is IObjectProxy objectProxy)
        {
            var result2 = objectProxy.TryGetObjectMember(instance, member, out value, isIndexedProperty);
            FinalizeValue(ref value);
            return result2;
        }

        if (ObjectProxyRegistry
                .TryGetObjectProxy(instance, out var proxy) &&
            proxy
                .TryGetObjectMember(instance, member,
                    out value, isIndexedProperty))
        {
            FinalizeValue(ref value);
            return true;
        }

        var result = DefaultObjectProxy
            .TryGetObjectMember(instance, member, out value, isIndexedProperty);
        FinalizeValue(ref value);
        return result;
    }

    private void FinalizeValue(ref object value)
    {
        if (Options.NoUndefined && value == Undefined.Value)
            value = null;
    }

    internal bool TrySetObjectMember(
        object instance,
        object member,
        object value,
        bool isIndexedProperty = false)
    {
        if (instance == null)
            return false;

        ProcessObjectMemberSecurityPolicy(instance, member);

        if (ObjectProxyRegistry
                .TryGetObjectProxy(instance, out var proxy) &&
            proxy
                .TrySetObjectMember(instance, member,
                    value, isIndexedProperty))
            return true;

        return DefaultObjectProxy
            .TrySetObjectMember(instance, member, value, isIndexedProperty);
    }

    internal void ProcessObjectMemberSecurityPolicy(object obj, object member)
    {
        if (obj == null || member == null)
            return;
        var disableReflection =
            !Options.SecurityPolicy.HasFlag(SecurityPolicy.EnableReflection);
        if (disableReflection)
        {
            var ns = obj.GetType().Namespace;
            if (ns != null && ns.StartsWith("System.Reflection", StringComparison.Ordinal))
                Exceptions.ThrowReflectionSecurityException(obj, member);
        }

        if (MemberAccessPolicy
            .IsObjectMemberAccessAllowed(obj, member.ToString()))
            return;

        Exceptions.ThrowMemberAccessSecurityException(obj, member);
    }
}

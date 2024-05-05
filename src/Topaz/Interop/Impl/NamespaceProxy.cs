using System;
using System.Collections.Generic;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop;

/// <summary>
/// Proxy object that enables access to namespaces.
/// </summary>
public sealed class NamespaceProxy : ITypeProxy
{
    private Dictionary<string, NamespaceProxy> SubnamespaceProxies = new();

    /// <summary>
    /// Namespace name. 
    /// eg: System.Collections
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The type names including namespaces that is allowed.
    /// If this is null, no whitelist check happens.
    /// </summary>
    public IReadOnlySet<string> Whitelist;

    /// <summary>
    /// Generic type search up to given parameter count
    /// </summary>
    public int MaxGenericTypeArgumentCount { get; }

    /// <summary>
    /// If enabled, sub namespaces are accessible.
    /// </summary>
    public bool AllowSubNamespaces { get; }

    /// <summary>
    /// ValueConverter to be passed to the created TypeProxy.
    /// </summary>
    public IValueConverter ValueConverter { get; }

    /// <summary>
    /// MemberInfoProvider to be passed to the created TypeProxy.
    /// </summary>
    public IMemberInfoProvider MemberInfoProvider { get; }

    /// <summary>
    /// ProxyOptions to be passed to created TypeProxy.
    /// </summary>
    public ProxyOptions ProxyOptions { get; }

    /// <summary>
    /// This parameter is always null for namespace.
    /// </summary>
    public Type ProxiedType { get; }

    /// <summary>
    /// If true, types of the current namespace are accessible.
    /// </summary>
    public bool EnableTypeRetrieval { get; set; }

    public NamespaceProxy(
        string name,
        IReadOnlySet<string> whitelist,
        bool allowSubNamespaces,
        IValueConverter valueConverter,
        IMemberInfoProvider memberInfoProvider,
        int maxGenericTypeArgumentCount = 5,
        ProxyOptions proxyOptions = ProxyOptions.Default)
    {
        Name = name;
        Whitelist = whitelist;
        AllowSubNamespaces = allowSubNamespaces;
        ValueConverter = valueConverter;
        MemberInfoProvider = memberInfoProvider;
        MaxGenericTypeArgumentCount = maxGenericTypeArgumentCount;
        ProxyOptions = proxyOptions;
    }

    public object CallConstructor(IReadOnlyList<object> args)
    {
        Exceptions.ThrowCanNotCallConstructor(Name);
        return null;
    }

    public bool TryGetStaticMember(
        object member,
        out object value,
        bool isIndexedProperty = false)
    {
        if (member is not string memberName)
        {
            Exceptions.ThrowCannotRetrieveMemberOfNamespace(Name, member);
            value = null;
            return false;
        }

        if (SubnamespaceProxies.TryGetValue(memberName, out var subProxy))
        {
            value = subProxy;
            return true;
        }

        var fullname = Name + "." + memberName;
        var type = FindType(fullname);
        if (type == null)
        {
            if (AllowSubNamespaces)
            {
                value = subProxy = new NamespaceProxy(fullname,
                                           Whitelist,
                                           true,
                                           ValueConverter,
                                           MemberInfoProvider,
                                           MaxGenericTypeArgumentCount,
                                           ProxyOptions);
                subProxy.EnableTypeRetrieval = true;
                SubnamespaceProxies.Add(memberName, subProxy);
                return true;
            }
            value = null;
            return false;
        }

        if (!EnableTypeRetrieval || !type.IsPublic)
        {
            value = null;
            return false;
        }

        if (Whitelist != null && !Whitelist.Contains(fullname))
        {
            value = null;
            return false;
        }

        value = TypeProxyUsingReflection.GetTypeProxy(type);
        if (value == null)
            value = new TypeProxyUsingReflection(type, ValueConverter, MemberInfoProvider, fullname, ProxyOptions);
        return true;
    }

    static Type SearchType(string typeName)
    {
        var type = Type.GetType(typeName, false, false);
        if (type != null) return type;
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName, false, false);
            if (type != null)
                return type;
        }
        return null;
    }

    private Type FindType(string fullname)
    {
        var type = SearchType(fullname);
        if (type != null)
            return type;
        // search for generic types.
        for (var i = 1; i < MaxGenericTypeArgumentCount; ++i)
        {
            type = SearchType(fullname + "`" + i);
            if (type != null)
                return type;
        }
        return null;
    }

    public bool TrySetStaticMember(
        object member,
        object value,
        bool isIndexedProperty = false)
    {
        Exceptions.ThrowCannotAssignAValueToANamespaceMember(Name, member);
        return false;
    }

    public override string ToString()
    {
        return Name;
    }

    public void AddSubNameSpaces(Span<string> parts,
        IReadOnlySet<string> whitelist,
        bool allowSubNamespaces)
    {
        if (parts.Length == 0)
        {
            EnableTypeRetrieval = true;
            return;
        }
        if (SubnamespaceProxies.TryGetValue(parts[0], out var proxy))
        {
            proxy.AddSubNameSpaces(parts.Slice(1), whitelist, allowSubNamespaces);
            return;
        }
        var subProxy = new NamespaceProxy(Name + "." + parts[0], whitelist, allowSubNamespaces && parts.Length == 1, ValueConverter, MemberInfoProvider, MaxGenericTypeArgumentCount);
        subProxy.AddSubNameSpaces(parts.Slice(1), whitelist, allowSubNamespaces);
        SubnamespaceProxies.Add(parts[0], subProxy);
    }
}

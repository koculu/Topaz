﻿using System;
using System.Collections.Generic;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop;

/// <summary>
/// Proxy object that enables access to namespaces.
/// </summary>
public sealed class NamespaceProxy : ITypeProxy
{
    /// <summary>
    /// Namespace name. 
    /// eg: System.Collections
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The type names including namespaces that is allowed.
    /// If this is null, no whitelist check happens.
    /// </summary>
    public ISet<string> Whitelist;

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

    public NamespaceProxy(
        string name,
        ISet<string> whitelist,
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

        var fullname = Name + "." + memberName;
        var type = FindType(fullname);
        if (type == null)
        {
            if (AllowSubNamespaces)
            {
                value = new NamespaceProxy(fullname,
                                           Whitelist,
                                           true,
                                           ValueConverter,
                                           MemberInfoProvider,
                                           MaxGenericTypeArgumentCount,
                                           ProxyOptions);
                return true;
            }
            value = null;
            return false;
        }

        if (!type.IsPublic)
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

    private Type FindType(string fullname)
    {
        var type = Type.GetType(fullname, false, false);
        if (type != null)
            return type;
        // search for generic types.
        for (var i = 1; i < MaxGenericTypeArgumentCount; ++i)
        {
            type = Type.GetType(fullname + "`" + i, false, false);
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
}

using System;
using System.Collections.Generic;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Interop;

public sealed class DictionaryObjectProxyRegistry : IObjectProxyRegistry
{
    readonly Dictionary<Type, IObjectProxy> proxyRegistryMap = new();

    public void AddObjectProxy(Type type, IObjectProxy proxy)
    {
        proxyRegistryMap.Add(type, proxy);
    }

    public void RemoveObjectProxy(Type type)
    {
        proxyRegistryMap.Remove(type);
    }

    public bool TryGetObjectProxy(object instance, out IObjectProxy proxy)
    {
        if (instance == null)
        {
            proxy = null;
            return false;
        }
        if (instance is JsArray)
        {
            proxy = JsArrayObjectProxy.Instance;
            return true;
        }
        return proxyRegistryMap.TryGetValue(instance.GetType(), out proxy);
    }
}

using System;

namespace Tenray.Topaz.Interop;

public sealed class GenericMethodSelector : IObjectProxy
{
    public ITypeProxy TypeProxy { get; }

    public IObjectProxy ObjectProxy { get; }

    public object Instance { get; }

    public Type[] Types { get; }

    public GenericMethodSelector(ITypeProxy typeProxy, object instance, Type[] types)
    {
        TypeProxy = typeProxy;
        Instance = instance;
        Types = types;
    }
    
    public GenericMethodSelector(IObjectProxy objectProxy, object instance, Type[] types)
    {
        ObjectProxy = objectProxy;
        Instance = instance;
        Types = types;
    }

    public static object GenericArguments(IObjectProxy proxyInstance, object instance, params Type[] types)
    {
        return new GenericMethodSelector(proxyInstance, instance, types);
    }

    public static object StaticGenericArguments(ITypeProxy proxyInstance, params Type[] types)
    {
        return new GenericMethodSelector(proxyInstance, null, types);
    }

    public bool TryGetObjectMember(object _, object member, out object value, bool isIndexedProperty = false)
    {
        if (ObjectProxy != null)
        {
            var result = ObjectProxy.TryGetObjectMember(Instance, member, out value, isIndexedProperty);
            if (Types != null && Types.Length > 0 && value is InvokerUsingReflection invoker)
            {
                invoker.GenericMethodArguments = Types;
            }
            return result;
        }
        if (TypeProxy != null)
        {
            var result = TypeProxy.TryGetStaticMember(member, out value, isIndexedProperty);
            if (Types != null && Types.Length > 0 && value is InvokerUsingReflection invoker)
            {
                invoker.GenericMethodArguments = Types;
            }
            return result;
        }
        value = null;
        return false;
    }

    public bool TrySetObjectMember(object _, object member, object value, bool isIndexedProperty = false)
    {
        if (ObjectProxy != null)
        {
            return ObjectProxy.TrySetObjectMember(Instance, member, value, isIndexedProperty);
        }
        if (TypeProxy != null)
        {
            return TypeProxy.TrySetStaticMember(member, value, isIndexedProperty);
        }
        return false;
    }
}

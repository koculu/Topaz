using System;
using Tenray.Topaz.Interop;

namespace Tenray.Topaz.API;

/// <summary>
/// Fast object proxy for JsArray. The remaining methods and properties would be accessed via default object proxy.
/// </summary>
internal class JsArrayObjectProxy : IObjectProxy
{
    public static IObjectProxy Instance = new JsArrayObjectProxy();

    public bool TryGetObjectMember(object instance, object member, out object value, bool isIndexedProperty = false)
    {
        var array = (JsArray)instance;
        if (isIndexedProperty)
        {
            return TryGetIndexedProperty(array, member, out value);
        }

        if (array.TryGetValue(member, out value))
            return true;

        if (member == null)
        {
            value = null;
            return false;
        }
        switch (member)
        {
            case "push":
                {
                    value = new JsArrayInvoker(array, JsArrayOptimizedMethods.push);
                    return true;
                }
            case "pop":
                {
                    value = new JsArrayInvoker(array, JsArrayOptimizedMethods.pop);
                    return true;
                }
            case "shift":
                {
                    value = new JsArrayInvoker(array, JsArrayOptimizedMethods.shift);
                    return true;
                }
        }
        value = null;
        return false;
    }

    internal static bool TryGetIndexedProperty(IJsArray array, object member, out object value)
    {
        if (member is double d)
        {
            var converted = Convert.ToInt32(d);
            if (converted == d)
            {
                return array.TryGetArrayValue(converted, out value);
            }
        }
        else if (member is long l)
        {
            return array.TryGetArrayValue((int)l, out value);
        }
        else if (member is int i)
        {
            return array.TryGetArrayValue(i, out value);
        }
        else if (member is string s && int.TryParse(s, out var parsedIndex))
        {
            return array.TryGetArrayValue(parsedIndex, out value);
        }
        value = null;
        return false;
    }

    public bool TrySetObjectMember(object instance, object member, object value, bool isIndexedProperty = false)
    {
        var array = (JsArray)instance;
        if (isIndexedProperty)
        {
            if (TrySetIndexedProperty(array, member, value))
                return true;
            array.SetValue(member, value);
            return true;
        }
        return false;
    }

    internal static bool TrySetIndexedProperty(IJsArray array, object member, object value)
    {
        if (member is double d)
        {
            var converted = Convert.ToInt32(d);
            if (converted == d)
            {
                array.SetArrayValue(converted, value);
                return true;
            }
        }
        else if (member is long l)
        {
            array.SetArrayValue((int)l, value);
            return true;
        }
        else if (member is int i)
        {
            array.SetArrayValue(i, value);
            return true;
        }
        else if (member is string s && int.TryParse(s, out var parsedIndex))
        {
            array.SetArrayValue(parsedIndex, value);
            return true;
        }
        return false;
    }
}
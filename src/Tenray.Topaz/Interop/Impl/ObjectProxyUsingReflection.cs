using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop
{
    public class ObjectProxyUsingReflection : IObjectProxy
    {
        public object Instance { get; }

        public Type ProxiedType { get; }

        public ProxyOptions ProxyOptions { get; }

        readonly PropertyInfo[] _indexedProperties;

        readonly ParameterInfo[][] _indexedPropertyParameters;

        public ObjectProxyUsingReflection(
            Type proxiedType,
            ProxyOptions proxyOptions = ProxyOptions.Default)
        {
            ProxiedType = proxiedType;
            ProxyOptions = proxyOptions;

            if (proxiedType != null &&
                proxyOptions.HasFlag(ProxyOptions.AllowProperty))
            {
                (_indexedProperties, _indexedPropertyParameters) =
                    IndexedPropertyMetaGetter
                    .GetIndexedPropertiesAndParameters(
                        proxiedType,
                        BindingFlags.Public | BindingFlags.Instance);
            }
        }

        public bool TryGetObjectMember(
            object instance,
            object member,
            out object value,
            bool isIndexedProperty = false)
        {
            value = null;
            if (instance == null)
                return false;
            var memberName = member as string;
            if (instance is IDictionary dic && memberName != null)
            {
                if (dic.Contains(memberName))
                {
                    value = dic[memberName];
                    return true;
                }
            }

            if (instance is IList list)
            {
                if (isIndexedProperty)
                {
                    if (member is double d)
                    {
                        var converted = Convert.ToInt32(d);
                        if (converted == d)
                        {
                            value = list[converted];
                            return true;
                        }
                    }
                    else if (member is long l)
                    {
                        value = list[(int)l];
                        return true;
                    }
                    else if (member is int i)
                    {
                        value = list[i];
                        return true;
                    }
                }
            }

            var instanceType = instance.GetType();

            var options = ProxyOptions;
            var allowProperty =
                options.HasFlag(ProxyOptions.AllowProperty);
            if (isIndexedProperty)
            {
                if (!allowProperty)
                    return false;

                var indexedProperties = _indexedProperties;
                var indexedPropertyParameters = _indexedPropertyParameters;
                if (ProxiedType != instanceType)
                {
                    (indexedProperties, indexedPropertyParameters) =
                        IndexedPropertyMetaGetter
                        .GetIndexedPropertiesAndParameters(
                            instanceType,
                            BindingFlags.Public | BindingFlags.Instance);
                }

                var convertStringsToEnum =
                    options.HasFlag(ProxyOptions.ConvertStringArgumentsToEnum);
                if (ArgumentMatcher
                    .TryFindBestMatchWithTypeConversion(
                    new[] { member },
                    indexedPropertyParameters,
                    convertStringsToEnum,
                    out var index,
                    out var convertedArgs))
                {
                    var indexedProperty = indexedProperties[index];
                    if (indexedProperty.GetMethod == null ||
                        indexedProperty.GetMethod.IsPrivate)
                        return false;
                    value = indexedProperties[index].GetValue(instance, convertedArgs);
                    return true;
                }
                return false;
            }

            if (memberName == null)
            {
                // member should be a string if we have reached this far.
                // This indicates wrong usage of the function, hence throws exception
                Exceptions.ThrowCannotRetrieveMemberOfType(instance.ToString(), member);
                return false;
            }

            var members = instanceType
                .GetMember(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (members.Length == 0)
                return false;
            var firstMember = members[0];
            if (firstMember is PropertyInfo property)
            {
                if (!allowProperty)
                    return false;
                if (property.GetMethod == null ||
                    property.GetMethod.IsPrivate)
                    return false;
                value = property.GetValue(instance);
                return true;
            }
            var allowField =
                options.HasFlag(ProxyOptions.AllowField);
            if (firstMember is FieldInfo field)
            {
                if (!allowField)
                    return false;
                value = field.GetValue(instance);
                return true;
            }
            var allowMethod =
                options.HasFlag(ProxyOptions.AllowMethod);
            if (!allowMethod)
                return false;
            var methods = members
                .Where(x => x is MethodInfo)
                .Cast<MethodInfo>()
                .ToArray();
            if (methods.Length == 0)
                return false;
            value = new InvokerUsingReflection(
                memberName, methods, instance, options);
            return true;
        }

        public bool TrySetObjectMember(
            object instance,
            object member,
            object value,
            bool isIndexedProperty = false)
        {
            if (instance == null)
                return false;
            var memberName = member as string;
            if (instance is IDictionary dic && memberName != null)
            {
                if (dic.Contains(memberName))
                {
                    dic[memberName] = value;
                    return true;
                }
                else
                {
                    dic.Add(memberName, value);
                    return true;
                }
            }

            if (instance is IList list)
            {
                if (isIndexedProperty)
                {
                    if (member is double d)
                    {
                        var converted = Convert.ToInt32(d);
                        if (converted == d)
                        {
                            list[converted] = value;
                            return true;
                        }
                    }
                    else if (member is long l)
                    {
                        list[(int)l] = value;
                        return true;
                    }
                    else if (member is int i)
                    {
                        list[i] = value;
                        return true;
                    }
                }
            }

            var instanceType = instance.GetType();

            var options = ProxyOptions;
            var allowProperty =
                options.HasFlag(ProxyOptions.AllowProperty);
            if (isIndexedProperty)
            {
                if (!allowProperty)
                    return false;

                var indexedProperties = _indexedProperties;
                var indexedPropertyParameters = _indexedPropertyParameters;
                if (ProxiedType != instanceType)
                {
                    (indexedProperties, indexedPropertyParameters) =
                        IndexedPropertyMetaGetter
                        .GetIndexedPropertiesAndParameters(
                            instanceType,
                            BindingFlags.Public | BindingFlags.Instance);
                }

                var convertStringsToEnum =
                    options.HasFlag(ProxyOptions.ConvertStringArgumentsToEnum);
                if (ArgumentMatcher
                    .TryFindBestMatchWithTypeConversion(
                    new[] { member },
                    indexedPropertyParameters,
                    convertStringsToEnum,
                    out var index,
                    out var convertedArgs))
                {
                    var indexedProperty = indexedProperties[index];
                    if (indexedProperty.SetMethod == null ||
                        indexedProperty.SetMethod.IsPrivate)
                        return false;
                    indexedProperties[index].SetValue(instance, value, convertedArgs);
                    return true;
                }
                return false;
            }

            if (memberName == null)
            {
                // member should be a string if we have reached this far.
                // This indicates wrong usage of the function, hence throws exception
                Exceptions.ThrowCannotRetrieveMemberOfType(instance.ToString(), member);
                return false;
            }

            var members = instanceType
                .GetMember(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (members.Length == 0)
                return false;
            var firstMember = members[0];
            if (firstMember is PropertyInfo property)
            {
                if (!allowProperty)
                    return false;
                if (property.SetMethod == null ||
                    property.SetMethod.IsPrivate)
                    return false;
                 property.SetValue(instance, value);
                return true;
            }
            var allowField =
                options.HasFlag(ProxyOptions.AllowField);
            if (firstMember is FieldInfo field)
            {
                if (!allowField)
                    return false;
                field.SetValue(instance, value);
                return true;
            }
            return false;
        }
    }
}

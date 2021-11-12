using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.API;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop
{
    public class ObjectProxyUsingReflection : IObjectProxy
    {
        public object Instance { get; }

        public Type ProxiedType { get; }
        
        public ExtensionMethodRegistry ExtensionMethodRegistry { get; }
        
        public IValueConverter ValueConverter { get; }

        public ProxyOptions ProxyOptions { get; }

        readonly PropertyInfo[] _indexedProperties;

        readonly ParameterInfo[][] _indexedPropertyParameters;

        readonly ConcurrentDictionary<Tuple<Type, object>, Func<object, object>> _cachedGetters = new();

        readonly ConcurrentDictionary<Tuple<Type, object>, Action<object, object>> _cachedSetters = new();

        public ObjectProxyUsingReflection(
            Type proxiedType,
            ExtensionMethodRegistry extensionMethodRegistry,
            IValueConverter valueConverter,
            ProxyOptions proxyOptions = ProxyOptions.Default)
        {
            ProxiedType = proxiedType;
            ExtensionMethodRegistry = extensionMethodRegistry;
            ValueConverter = valueConverter;
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
            var isJsObject = false;
            if (instance is IJsObject jsObject)
            {
                isJsObject = true;
                if (!jsObject.IsPrototypeProperty(member))
                {
                    if (isIndexedProperty && instance is IJsArray jsArray)
                    {
                        if (member is double d)
                        {
                            var converted = Convert.ToInt32(d);
                            if (converted == d)
                            {
                                return jsArray.TryGetArrayValue(converted, out value);
                            }
                        }
                        else if (member is long l)
                        {
                            return jsArray.TryGetArrayValue((int)l, out value);
                        }
                        else if (member is int i)
                        {
                            return jsArray.TryGetArrayValue(i, out value);
                        }
                        else if (member is string s && int.TryParse(s, out var parsedIndex))
                        {
                            return jsArray.TryGetArrayValue(parsedIndex, out value);
                        }
                    }
                    if (jsObject.TryGetValue(member, out value))
                        return true;
                }
            }
            else
            {
                if (instance is IDictionary dic)
                {
                    if (dic.Contains(member))
                    {
                        value = dic[member];
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

            }

            var instanceType = instance.GetType();

            var options = ProxyOptions;
            var allowProperty =
                options.HasFlag(ProxyOptions.AllowProperty);
            if (!isJsObject && isIndexedProperty)
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

                if (ArgumentMatcher
                    .TryFindBestMatchWithTypeConversion(
                    ValueConverter,
                    new[] { member },
                    indexedPropertyParameters,
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

            var memberName = member as string;
            if (memberName == null)
            {
                // member should be a string if we have reached this far.
                // This indicates wrong usage of the function, hence throws exception
                Exceptions.ThrowCannotRetrieveMemberOfType(instance.ToString(), member);
                return false;
            }
            var cacheKey = Tuple.Create(instanceType, member);
            if (_cachedGetters.TryGetValue(cacheKey, out var cachedGetter))
            {
                value = cachedGetter(instance);
                return true;
            }
            var members = instanceType
                .GetMember(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (members.Length == 0)
            {
                var extMethodParameterInfo = ExtensionMethodRegistry
                    .GetMethodAndParameterInfo(memberName);
                if (!extMethodParameterInfo.HasAny)
                {
                    return false;
                }
                var methodGetter2 = new Func<object, object>((instance) =>
                {
                    return new InvokerUsingReflection(
                        memberName, Array.Empty<MethodInfo>(), instance, options, ValueConverter, extMethodParameterInfo);
                });
                _cachedGetters.TryAdd(cacheKey, methodGetter2);
                value = methodGetter2(instance);
                return true;
            }
            var firstMember = members[0];
            if (firstMember is PropertyInfo property)
            {
                if (!allowProperty)
                    return false;
                if (property.GetMethod == null ||
                    property.GetMethod.IsPrivate)
                    return false;
                
                var getter = new Func<object, object>((instance) =>
                {
                    return property.GetValue(instance);
                });
                _cachedGetters.TryAdd(cacheKey, getter);
                value = property.GetValue(instance);
                return true;
            }
            var allowField =
                options.HasFlag(ProxyOptions.AllowField);
            if (firstMember is FieldInfo field)
            {
                if (!allowField)
                    return false;
                var getter = new Func<object, object>((instance) =>
                {
                    return field.GetValue(instance);
                });
                _cachedGetters.TryAdd(cacheKey, getter);
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
            var extMethodParameterInfo2 = ExtensionMethodRegistry.GetMethodAndParameterInfo(memberName);
            if (methods.Length == 0 && !extMethodParameterInfo2.HasAny)
            {
                return false;
            }
            var methodGetter = new Func<object, object>((instance) =>
            {
                return new InvokerUsingReflection(memberName, methods, instance, options, ValueConverter, extMethodParameterInfo2);
            });
            _cachedGetters.TryAdd(cacheKey, methodGetter); 
            value = methodGetter(instance);
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

            var isJsObject = false;
            if (instance is IJsObject jsObject)
            {
                isJsObject = true;
                if (!jsObject.IsPrototypeProperty(member))
                {
                    if (isIndexedProperty && instance is IJsArray jsArray)
                    {
                        if (member is double d)
                        {
                            var converted = Convert.ToInt32(d);
                            if (converted == d)
                            {
                                jsArray.SetArrayValue(converted, value);
                                return true;
                            }
                        }
                        else if (member is long l)
                        {
                            jsArray.SetArrayValue((int)l, value);
                            return true;
                        }
                        else if (member is int i)
                        {
                            jsArray.SetArrayValue(i, value);
                            return true;
                        }
                        else if (member is string s && int.TryParse(s, out var parsedIndex))
                        {
                            jsArray.SetArrayValue(parsedIndex, value);
                            return true;
                        }
                    }
                    jsObject.SetValue(member, value);
                    return true;
                }
            }
            else
            {
                if (instance is IDictionary dic)
                {
                    if (dic.Contains(member))
                    {
                        dic[member] = value;
                        return true;
                    }
                    else
                    {
                        dic.Add(member, value);
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
            }

            var instanceType = instance.GetType();

            var options = ProxyOptions;
            var allowProperty =
                options.HasFlag(ProxyOptions.AllowProperty);
            if (!isJsObject && isIndexedProperty)
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

                if (ArgumentMatcher
                    .TryFindBestMatchWithTypeConversion(
                    ValueConverter,
                    new[] { member },
                    indexedPropertyParameters,
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

            var memberName = member as string;
            if (memberName == null)
            {
                // member should be a string if we have reached this far.
                // This indicates wrong usage of the function, hence throws exception
                Exceptions.ThrowCannotRetrieveMemberOfType(instance.ToString(), member);
                return false;
            }

            var cacheKey = Tuple.Create(instanceType, member);
            if (_cachedSetters.TryGetValue(cacheKey, out var cachedSetter))
            {
                cachedSetter(instance, value);
                return true;
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
                var action = new Action<object, object>((instance, value) =>
                {
                    if (ValueConverter
                        .TryConvertValue(value, property.PropertyType, out var convertedValue))
                        property.SetValue(instance, convertedValue);
                    else
                        Exceptions.CannotConvertValueToTargetType(value, property.PropertyType);
                });
                _cachedSetters.TryAdd(cacheKey, action);
                action(instance, value);
                return true;
            }
            var allowField =
                options.HasFlag(ProxyOptions.AllowField);
            if (firstMember is FieldInfo field)
            {
                if (!allowField)
                    return false;

                var action = new Action<object, object>((instance, value) =>
                {
                    if (ValueConverter
                        .TryConvertValue(value, field.FieldType, out var convertedValue))
                        field.SetValue(instance, convertedValue);
                    else
                        Exceptions.CannotConvertValueToTargetType(value, field.FieldType);
                });
                _cachedSetters.TryAdd(cacheKey, action);
                action(instance, value);
                return true;
            }
            return false;
        }
    }
}

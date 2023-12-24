using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.API;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop;

public sealed class ObjectProxyUsingReflection : IObjectProxy
{
    private sealed class CacheKey
    {
        public Type Type;

        public object Value;

        public CacheKey(Type type, object value)
        {
            Type = type;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is CacheKey key &&
                   Type == key.Type &&
                   Value == key.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }
    }

    public object Instance { get; }

    public Type ProxiedType { get; }

    public ExtensionMethodRegistry ExtensionMethodRegistry { get; }

    public IValueConverter ValueConverter { get; }

    public IMemberInfoProvider MemberInfoProvider { get; }

    public ProxyOptions ProxyOptions { get; }

    readonly PropertyInfo[] _indexedProperties;

    readonly ParameterInfo[][] _indexedPropertyParameters;

    readonly ConcurrentDictionary<CacheKey, Func<object, object>> _cachedGetters = new();

    readonly ConcurrentDictionary<CacheKey, Action<object, object>> _cachedSetters = new();

    private const string GENERIC_ARGUMENTS_METHOD = "GenericArguments";

    readonly static MethodAndParameterInfo genericMethodSelectorParameterInfo = new MethodAndParameterInfo(
                        new MethodInfo[1]
                        {
                            typeof(GenericMethodSelector).GetMethod(GENERIC_ARGUMENTS_METHOD)
                        },
                        new ParameterInfo[1][] {
                            typeof(GenericMethodSelector).GetMethod(GENERIC_ARGUMENTS_METHOD).GetParameters()
                        });

    public ObjectProxyUsingReflection(
        Type proxiedType,
        ExtensionMethodRegistry extensionMethodRegistry,
        IValueConverter valueConverter,
        IMemberInfoProvider memberInfoProvider,
        ProxyOptions proxyOptions = ProxyOptions.Default)
    {
        ProxiedType = proxiedType;
        ExtensionMethodRegistry = extensionMethodRegistry;
        ValueConverter = valueConverter;
        MemberInfoProvider = memberInfoProvider;
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
                    if (JsArrayObjectProxy.TryGetIndexedProperty(jsArray, member, out value))
                        return true;
                    if (value != null)
                        return false;
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
            else if (isIndexedProperty)
            {
                if (instance is IList list)
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

        if (member is not string memberName)
        {
            // member should be a string if we have reached this far.
            // This indicates wrong usage of the function, hence throws exception
            Exceptions.ThrowCannotRetrieveMemberOfType(instance.ToString(), member);
            return false;
        }

        var cacheKey = new CacheKey(instanceType, member);
        if (_cachedGetters.TryGetValue(cacheKey, out var cachedGetter))
        {
            value = cachedGetter(instance);
            return true;
        }
        var members = MemberInfoProvider.GetInstanceMembers(instance, memberName);
        if (members.Length == 0)
        {
            if (!options.HasFlag(ProxyOptions.AllowMethod))
                return false;
            var extMethodParameterInfo = ExtensionMethodRegistry
                .GetMethodAndParameterInfo(memberName);
            if (!extMethodParameterInfo.HasAny)
            {
                if (memberName == GENERIC_ARGUMENTS_METHOD)
                {
                    var invContext = new InvokerUsingReflectionContext(memberName,
                                                       Array.Empty<MethodInfo>(),
                                                       options,
                                                       ValueConverter,
                                                       genericMethodSelectorParameterInfo).ToInvoker(this);
                    invContext.AttachedParameter = instance;
                    value = invContext;
                    return true;
                }
                return false;
            }

            var invokerContext2 = new InvokerUsingReflectionContext(
                    memberName, Array.Empty<MethodInfo>(), options, ValueConverter, extMethodParameterInfo);
            var methodGetter2 = new Func<object, object>((instance) =>
            {
                return invokerContext2.ToInvoker(instance);
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
            if (memberName == GENERIC_ARGUMENTS_METHOD)
            {
                var invContext = new InvokerUsingReflectionContext(memberName,
                                                   Array.Empty<MethodInfo>(),
                                                   options,
                                                   ValueConverter,
                                                   genericMethodSelectorParameterInfo)
                    .ToInvoker(this);
                invContext.AttachedParameter = instance;
                value = invContext;
                return true;
            }
            return false;
        }

        var invokerContext = new InvokerUsingReflectionContext(memberName, methods, options, ValueConverter, extMethodParameterInfo2);
        var methodGetter = new Func<object, object>((instance) =>
        {
            return invokerContext.ToInvoker(instance);
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
                    if (JsArrayObjectProxy.TrySetIndexedProperty(jsArray, member, value))
                        return true;
                }
                jsObject.SetValue(member, value);
                return true;
            }
        }
        else
        {
            if (instance is IDictionary dic)
            {
                var convertedValue = value;
                var dicType = dic.GetType();
                if (dicType.IsGenericType && dicType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var valueType = dicType.GenericTypeArguments[1];
                    if (!ValueConverter.TryConvertValue(value, valueType, out convertedValue))
                        Exceptions.CannotConvertValueToTargetType(value, valueType);
                }
                if (dic.Contains(member))
                {
                    dic[member] = convertedValue;
                    return true;
                }
                else
                {
                    dic.Add(member, convertedValue);
                    return true;
                }
            }
            if (instance is IList list)
            {
                if (isIndexedProperty)
                {
                    var convertedValue = value;
                    var listType = list.GetType();
                    if (listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var valueType = listType.GenericTypeArguments[0];
                        if (!ValueConverter.TryConvertValue(value, valueType, out convertedValue))
                            Exceptions.CannotConvertValueToTargetType(value, valueType);
                    }

                    if (member is double d)
                    {
                        var converted = Convert.ToInt32(d);
                        if (converted == d)
                        {
                            list[converted] = convertedValue;
                            return true;
                        }
                    }
                    else if (member is long l)
                    {
                        list[(int)l] = convertedValue;
                        return true;
                    }
                    else if (member is int i)
                    {
                        list[i] = convertedValue;
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

        if (member is not string memberName)
        {
            // member should be a string if we have reached this far.
            // This indicates wrong usage of the function, hence throws exception
            Exceptions.ThrowCannotRetrieveMemberOfType(instance.ToString(), member);
            return false;
        }

        var cacheKey = new CacheKey(instanceType, member);
        if (_cachedSetters.TryGetValue(cacheKey, out var cachedSetter))
        {
            cachedSetter(instance, value);
            return true;
        }

        var members = MemberInfoProvider.GetInstanceMembers(instance, memberName);
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

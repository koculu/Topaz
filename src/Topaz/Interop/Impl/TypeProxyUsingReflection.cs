﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop;

public sealed class TypeProxyUsingReflection : ITypeProxy
{
    public IValueConverter ValueConverter { get; }

    public IMemberInfoProvider MemberInfoProvider { get; }

    public string Name { get; }

    public Type ProxiedType { get; }

    public ProxyOptions ProxyOptions { get; }

    readonly ConstructorInfo[] _constructors;

    readonly ParameterInfo[][] _constructorParameters;

    readonly PropertyInfo[] _indexedProperties;

    readonly ParameterInfo[][] _indexedPropertyParameters;

    readonly ConcurrentDictionary<object, Func<object>> _cachedGetters = new();

    readonly ConcurrentDictionary<object, Action<object>> _cachedSetters = new();

    static readonly ConcurrentDictionary<Type, TypeProxyUsingReflection> CreatedTypeProxies = new();

    private const string GENERIC_ARGUMENTS_METHOD = "GenericArguments";

    private const string STATIC_GENERIC_ARGUMENTS_METHOD = "StaticGenericArguments";

    readonly static MethodAndParameterInfo genericMethodSelectorParameterInfo = new MethodAndParameterInfo(
                        new MethodInfo[1]
                        {
                            typeof(GenericMethodSelector).GetMethod(STATIC_GENERIC_ARGUMENTS_METHOD)
                        },
                        new ParameterInfo[1][] {
                            typeof(GenericMethodSelector).GetMethod(STATIC_GENERIC_ARGUMENTS_METHOD).GetParameters()
                        });

    public TypeProxyUsingReflection(
        Type proxiedType,
        IValueConverter valueConverter,
        IMemberInfoProvider memberInfoProvider,
        string name = null,
        ProxyOptions proxyOptions = ProxyOptions.Default)
    {
        Name = name ?? proxiedType.FullName;
        ValueConverter = valueConverter;
        MemberInfoProvider = memberInfoProvider;
        ProxiedType = proxiedType;
        ProxyOptions = proxyOptions;
        if (proxyOptions.HasFlag(ProxyOptions.AllowConstructor) &&
            proxyOptions.HasFlag(ProxyOptions.AutomaticTypeConversion))
        {
            _constructors = proxiedType.GetConstructors(
                BindingFlags.Public | BindingFlags.Instance);
            _constructorParameters = _constructors
                .Select(x => x.GetParameters())
                .ToArray();
        }

        if (proxyOptions.HasFlag(ProxyOptions.AllowProperty))
        {
            {
                (_indexedProperties, _indexedPropertyParameters) =
                    IndexedPropertyMetaGetter
                    .GetIndexedPropertiesAndParameters(
                        proxiedType,
                        BindingFlags.Public | BindingFlags.Static);
            }
        }
        if (proxiedType != null)
            CreatedTypeProxies.TryAdd(proxiedType, this);
    }

    public object CallConstructor(IReadOnlyList<object> args)
    {
        var type = ProxiedType;
        if (type.IsGenericType)
            return CallGenericConstructor(type, args);
        return CallNonGenericConstructor(type, args);
    }

    private object CallNonGenericConstructor(Type type, IReadOnlyList<object> args)
    {
        var options = ProxyOptions;
        if (!options.HasFlag(ProxyOptions.AllowConstructor))
            Exceptions.ThrowCanNotCallConstructor(type);

        if (type.IsSubclassOf(typeof(Delegate)))
            return CreateDelegate(type, args);

        if (args.Count == 0)
            return Activator.CreateInstance(type);

        if (!options.HasFlag(ProxyOptions.AutomaticTypeConversion))
            return Activator.CreateInstance(type, args);
        var constructors = _constructors;
        var constructorParameters = _constructorParameters;
        if (type != ProxiedType)
        {
            constructors = type.GetConstructors(
                BindingFlags.Public | BindingFlags.Instance);
            constructorParameters = constructors
                .Select(x => x.GetParameters())
                .ToArray();
        }
        if (ArgumentMatcher.TryFindBestMatchWithTypeConversion(
            ValueConverter,
            args,
            constructorParameters,
            out var index,
            out var convertedArgs))
        {
            var bestConstructor = constructors[index];
            return bestConstructor.Invoke(convertedArgs);
        }
        Exceptions
            .ThrowCanNotCallConstructorWithGivenArguments(
            Name, args);
        return null;
    }

    private object CreateDelegate(Type type, IReadOnlyList<object> args)
    {
        var returnType = type.GetMethod("Invoke").ReturnType;
        var argTypes = type.GetTypeInfo().GenericTypeArguments;
        if (args == null || args.Count == 0)
            throw new TopazException("Cannot create delegate with no arguments.");
        var arg = args[0];
        if (arg is not TopazFunction topazFunction)
            throw new TopazException("Delegate constructor requires Javascript function.");

        return DynamicDelagateFactory.CreateDynamicDelegate(argTypes, returnType,
            (x) => topazFunction.Execute(x, default), ValueConverter);
    }

    private object CallGenericConstructor(Type type, IReadOnlyList<object> args)
    {
        var len = type.GetTypeInfo().GenericTypeParameters.Length;
        var genericType = type.MakeGenericType(
            args.Take(len)
            .Select(x =>
            {
                if (x is Type type)
                    return type;
                if (x is ITypeProxy typeProxy && typeProxy != null)
                    return typeProxy.ProxiedType;
                Exceptions.ThrowCanNotCallConstructor(this);
                return null;
            }).ToArray());
        args = args.Skip(len).ToArray();
        return CallNonGenericConstructor(genericType, args);
    }

    public bool TryGetStaticMember(
        object member,
        out object value,
        bool isIndexedProperty = false)
    {
        var options = ProxyOptions;
        var allowProperty =
            options.HasFlag(ProxyOptions.AllowProperty);
        value = null;
        if (isIndexedProperty)
        {
            if (!allowProperty)
                return false;
            if (ArgumentMatcher
                .TryFindBestMatchWithTypeConversion(
                ValueConverter,
                new[] { member },
                _indexedPropertyParameters,
                out var index,
                out var convertedArgs))
            {
                var indexedProperty = _indexedProperties[index];
                if (indexedProperty.GetMethod == null ||
                    indexedProperty.GetMethod.IsPrivate)
                    return false;
                value = _indexedProperties[index].GetValue(null, convertedArgs);
                return true;
            }
            return false;
        }

        if (member is not string memberName)
        {
            // member should be a string if we have reached this far.
            // This indicates wrong usage of the function, hence throws exception
            Exceptions.ThrowCannotRetrieveMemberOfType(Name, member);
            return false;
        }

        if (_cachedGetters.TryGetValue(member, out var cachedGetter))
        {
            value = cachedGetter();
            return true;
        }
        var members = MemberInfoProvider.GetStaticMembers(ProxiedType, memberName);
        if (members.Length == 0)
        {
            if (memberName == GENERIC_ARGUMENTS_METHOD)
            {
                value = new InvokerUsingReflectionContext(memberName,
                                                   Array.Empty<MethodInfo>(),
                                                   options,
                                                   ValueConverter,
                                                   genericMethodSelectorParameterInfo).ToInvoker(this);
                return true;
            }
            return false;
        }
        var firstMember = members[0];
        if (firstMember is PropertyInfo property)
        {
            if (!allowProperty)
                return false;
            if (property.GetMethod == null ||
                property.GetMethod.IsPrivate)
                return false;
            var getter = new Func<object>(() =>
            {
                return property.GetValue(null);
            });
            _cachedGetters.TryAdd(member, getter);
            value = property.GetValue(null);
            return true;
        }
        var allowField =
            options.HasFlag(ProxyOptions.AllowField);
        if (firstMember is FieldInfo field)
        {
            if (!allowField)
                return false;
            var getter = new Func<object>(() =>
            {
                return field.GetValue(null);
            });
            _cachedGetters.TryAdd(member, getter);
            value = field.GetValue(null);
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
        {
            if (memberName == GENERIC_ARGUMENTS_METHOD)
            {
                value = new InvokerUsingReflectionContext(memberName,
                                                   Array.Empty<MethodInfo>(),
                                                   options,
                                                   ValueConverter,
                                                   genericMethodSelectorParameterInfo).ToInvoker(this);
                return true;
            }
            return false;
        }

        var invokerContext =
            new InvokerUsingReflectionContext(Name + "." + memberName, methods, options, ValueConverter);
        var methodGetter = new Func<object>(() =>
        {
            return invokerContext.ToInvoker(null);
        });
        _cachedGetters.TryAdd(member, methodGetter);
        value = methodGetter();
        return true;
    }

    public override string ToString()
    {
        return Name;
    }

    public bool TrySetStaticMember(
        object member,
        object value,
        bool isIndexedProperty = false)
    {
        var options = ProxyOptions;
        var allowProperty =
            options.HasFlag(ProxyOptions.AllowProperty);
        if (isIndexedProperty)
        {
            if (!allowProperty)
                return false;
            if (ArgumentMatcher
                .TryFindBestMatchWithTypeConversion(
                ValueConverter,
                new[] { member },
                _indexedPropertyParameters,
                out var index,
                out var convertedArgs))
            {
                var indexedProperty = _indexedProperties[index];
                if (indexedProperty.SetMethod == null ||
                    indexedProperty.SetMethod.IsPrivate)
                    return false;
                _indexedProperties[index].SetValue(null, value, convertedArgs);
                return true;
            }
            return false;
        }

        if (member is not string memberName)
        {
            // member should be a string if we have reached this far.
            // This indicates wrong usage of the function, hence throws exception
            Exceptions.ThrowCannotRetrieveMemberOfType(Name, member);
            return false;
        }

        if (_cachedSetters.TryGetValue(member, out var cachedSetter))
        {
            cachedSetter(value);
            return true;
        }

        var members = MemberInfoProvider.GetStaticMembers(ProxiedType, memberName);
        if (members.Length == 0)
            return false;
        var firstMember = members[0];
        if (firstMember is PropertyInfo property)
        {
            if (!allowProperty)
                return false;
            if (property.SetMethod == null || property.SetMethod.IsPrivate)
                return false;

            var action = new Action<object>((value) =>
            {
                if (ValueConverter
                       .TryConvertValue(value, property.PropertyType, out var convertedValue))
                    property.SetValue(null, convertedValue);
                else
                    Exceptions.CannotConvertValueToTargetType(value, property.PropertyType);
            });
            _cachedSetters.TryAdd(member, action);
            action(value);
            return true;
        }
        var allowField =
            options.HasFlag(ProxyOptions.AllowField);
        if (firstMember is FieldInfo field)
        {
            if (!allowField)
                return false;

            var action = new Action<object>((value) =>
            {
                if (ValueConverter
                    .TryConvertValue(value, field.FieldType, out var convertedValue))
                    field.SetValue(null, convertedValue);
                else
                    Exceptions.CannotConvertValueToTargetType(value, field.FieldType);
            });
            _cachedSetters.TryAdd(member, action);
            action(value);
            return true;
        }
        return false;
    }

    public static object GetTypeProxy(Type type)
    {
        CreatedTypeProxies.TryGetValue(type, out var proxy);
        return proxy;
    }
}

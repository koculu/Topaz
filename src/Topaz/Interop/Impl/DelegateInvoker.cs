﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop;

public sealed class DelegateInvoker : IDelegateInvoker
{
    static readonly
        ConcurrentDictionary<
            Tuple<Type, string>,
            MethodInfo
            > methods = new();

    public IValueConverter ValueConverter { get; }

    public bool AutomaticArgumentConversion { get; set; }

    public DelegateInvoker(
        IValueConverter valueConverter,
        bool automaticArgumentConversion = true)
    {
        ValueConverter = valueConverter;
        AutomaticArgumentConversion = automaticArgumentConversion;
    }

    public object Invoke(
        object function,
        IReadOnlyList<object> args)
    {
        if (function is Delegate)
        {
            return InvokeMethodByName(function, "Invoke", args);
        }
        return Exceptions.ThrowCannotCallFunction(function);
    }

    private object InvokeMethodByName(
        object value,
        string methodName,
        IReadOnlyList<object> args)
    {
        var valueType = value.GetType();
        var key = Tuple.Create(valueType, methodName);
        var method = methods.GetOrAdd(key, (key) =>
        {
            var method = valueType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            return method;
        });
        var parameters = method.GetParameters();
        var len = parameters.Length;
        var argsArray = args as object[] ?? args.ToArray();
        if (AutomaticArgumentConversion)
        {
            if (ArgumentMatcher.
                TryFindBestMatchWithTypeConversion(
                    ValueConverter,
                    args,
                    parameters,
                    out var convertedArgs))
            {
                argsArray = convertedArgs;
            }
            else
            {
                Exceptions.ThrowCannotCallDelegateArgumentMismatch(args);
            }
        }
        return method.Invoke(value, argsArray);
    }
}

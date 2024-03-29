﻿using Esprima.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions;

internal static partial class ArrayPatternHandler
{
    // https://hacks.mozilla.org/2015/05/es6-in-depth-destructuring/
    internal static object ProcessArrayPattern(
        ScriptExecutor scriptExecutor,
        ArrayPattern arrayPattern,
        object value,
        Action<object, object, CancellationToken> callback,
        CancellationToken token)
    {
        if (value is not IEnumerable enumerable)
            return Exceptions.ThrowValueIsNotEnumerable(value); 
        var enumerator = enumerable.GetEnumerator();
        var elements = arrayPattern.Elements;
        var len = arrayPattern.Elements.Count;
        for (var i = 0; i < len; ++i)
        {
            if (!enumerator.MoveNext())
                return value;
            token.ThrowIfCancellationRequested();
            var item = enumerator.Current;
            var el = elements[i];
            if (el == null)
                continue;
            if (el is RestElement rest)
            {
                if (i != len - 1)
                    Exceptions.ThrowRestElementMustBeLastElement();
                var restArray = new List<object>();
                do
                {
                    token.ThrowIfCancellationRequested();
                    restArray.Add(enumerator.Current);
                }
                while (enumerator.MoveNext());
                callback(rest.Argument, restArray, token);
                return value;
            }
            if (el is ArrayPattern nestedArrayPattern)
            {
                ProcessArrayPattern(
                    scriptExecutor,
                    nestedArrayPattern,
                    item,
                    callback,
                    token);
                continue;
            }
            else if (el is ObjectPattern nestedObjectPattern)
            {
                ObjectPatternHandler.ProcessObjectPattern(
                    scriptExecutor,
                    nestedObjectPattern,
                    item,
                    callback,
                    token);
                continue;
            }
            callback(el, item, token);
        }
        return value;
    }
}

﻿using Esprima.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions
{
    internal static partial class ArrayPatternHandler
    {
        // https://hacks.mozilla.org/2015/05/es6-in-depth-destructuring/
        internal async static ValueTask<object> ProcessArrayPatternAsync(
            ScriptExecutor scriptExecutor,
            ArrayPattern arrayPattern,
            object value,
            Func<object, object, ValueTask> callback)
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
                        restArray.Add(enumerator.Current);
                    }
                    while (enumerator.MoveNext());
                    await callback(rest.Argument, restArray);
                    return value;
                }
                if (el is ArrayPattern nestedArrayPattern)
                {
                    await ProcessArrayPatternAsync(
                        scriptExecutor,
                        nestedArrayPattern,
                        item,
                        callback);
                    continue;
                }
                else if (el is ObjectPattern nestedObjectPattern)
                {
                    await ObjectPatternHandler.ProcessObjectPatternAsync(
                        scriptExecutor,
                        nestedObjectPattern,
                        item,
                        callback);
                    continue;
                }
                await callback(el, item);
            }
            return value;
        }
    }
}
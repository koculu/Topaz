using Esprima.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions
{
    internal static partial class ArrayPatternHandler
    {
        // https://hacks.mozilla.org/2015/05/es6-in-depth-destructuring/
        internal static object ProcessArrayPattern(
            ScriptExecutor scriptExecutor,
            ArrayPattern arrayPattern,
            object value,
            Action<object, object> callback)
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
                    callback(rest.Argument, restArray);
                    return value;
                }
                if (el is ArrayPattern nestedArrayPattern)
                {
                    ProcessArrayPattern(
                        scriptExecutor,
                        nestedArrayPattern,
                        item,
                        callback);
                    continue;
                }
                else if (el is ObjectPattern nestedObjectPattern)
                {
                    ObjectPatternHandler.ProcessObjectPattern(
                        scriptExecutor,
                        nestedObjectPattern,
                        item,
                        callback);
                    continue;
                }
                callback(el, item);
            }
            return value;
        }
    }
}

﻿using Esprima.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.API;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions;

internal static partial class ArrayExpressionHandler
{
    internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
    {
        var expr = (ArrayExpression)expression;
        var elements = expr.Elements;
        var len = elements.Count;
        var result = new JsArray();
        if (len == 0)
            return result;
        for (var i = 0; i < len; ++i)
        {
            token.ThrowIfCancellationRequested();
            var el = elements[i];
            if (el is SpreadElement spreadElement)
            {
                var value = await scriptExecutor
                    .ExecuteExpressionAndGetValueAsync(spreadElement.Argument, token);
                if (value is not IEnumerable enumerable)
                {
                    return Exceptions.ThrowValueIsNotEnumerable(value);
                }
                foreach (var item in enumerable)
                {
                    token.ThrowIfCancellationRequested();
                    result.AddArrayValue(item);
                }
                continue;
            }
            result.AddArrayValue(await scriptExecutor.ExecuteStatementAsync(el, token));
        }
        // We cannot return salt array or
        // unwrap here as assignment pattern can define variables
        // in an array.
        // Unwrap will happen in the first GetValue statement once.
        // And this is the stage where the array is actually used in an expression.            
        return new TopazArrayWrapper(scriptExecutor, result);
    }
}

using Esprima.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions
{
    internal class ArrayExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (ArrayExpression)expression;
            var elements = expr.Elements;
            var len = elements.Count;
            if (len == 0)
                return Array.Empty<object>();
            var result = new List<object>(len);
            for (var i = 0; i < len; ++i)
            {
                var el = elements[i];
                if (el is SpreadElement spreadElement)
                {
                    var value = scriptExecutor
                        .ExecuteExpressionAndGetValue(spreadElement.Argument);
                    if (value is not IEnumerable enumerable)
                    {
                        return Exceptions.ThrowValueIsNotEnumerable(value);
                    }
                    foreach (var item in enumerable)
                    {
                        result.Add(item);
                    }
                    continue;
                }
                result.Add(scriptExecutor.ExecuteStatement(el));
            }
            // We cannot return salt array or
            // unwrap here as assignment pattern can define variables
            // in an array.
            // Unwrap will happen in the first GetValue statement once.
            // And this is the stage where the array is actually used in an expression.            
            return new TopazArrayWrapper(scriptExecutor, result);
        }
    }
}

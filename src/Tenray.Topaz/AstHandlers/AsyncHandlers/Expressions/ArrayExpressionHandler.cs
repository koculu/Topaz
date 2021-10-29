using Esprima.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions
{
    internal static partial class ArrayExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression)
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
                    var value = await scriptExecutor
                        .ExecuteExpressionAndGetValueAsync(spreadElement.Argument);
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
                result.Add(await scriptExecutor.ExecuteStatementAsync(el));
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

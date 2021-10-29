using Esprima.Ast;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class CallExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (CallExpression)expression;
            var calleeArgs = expr.Arguments;
            var len = calleeArgs.Count;
            var args = new List<object>(len);
            for (var i = 0; i < len; ++i)
            {
                var arg = calleeArgs[i];
                if (arg is SpreadElement spreadElement)
                {
                    var inner = await scriptExecutor.ExecuteExpressionAndGetValueAsync(spreadElement.Argument);
                    if (inner is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            args.Add(item);
                        }
                    }
                    continue;
                }
                args.Add(await scriptExecutor.ExecuteExpressionAndGetValueAsync(arg));
            }
            var callee = await scriptExecutor.ExecuteStatementAsync(expr.Callee);
            var result = await scriptExecutor.CallFunctionAsync(callee, args, expr.Optional);            
            return result;
        }
    }
}

using Esprima.Ast;
using System.Collections;
using System.Collections.Generic;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal class CallExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
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
                    var inner = scriptExecutor.ExecuteExpressionAndGetValue(spreadElement.Argument);
                    if (inner is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            args.Add(item);
                        }
                    }
                    continue;
                }
                args.Add(scriptExecutor.ExecuteExpressionAndGetValue(arg));
            }
            var callee = scriptExecutor.ExecuteStatement(expr.Callee);
            return scriptExecutor.CallFunction(callee, args, expr.Optional);
        }
    }
}

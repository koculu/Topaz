using Esprima.Ast;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class CallExpressionHandler
{
    internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
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
                var inner = scriptExecutor.ExecuteExpressionAndGetValue(spreadElement.Argument, token);
                if (inner is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        token.ThrowIfCancellationRequested();
                        args.Add(item);
                    }
                }
                continue;
            }
            args.Add(scriptExecutor.ExecuteExpressionAndGetValue(arg, token));
        }
        token.ThrowIfCancellationRequested();
        var callee = scriptExecutor.ExecuteStatement(expr.Callee, token);
        return scriptExecutor.CallFunction(callee, args, expr.Optional, token);
    }
}

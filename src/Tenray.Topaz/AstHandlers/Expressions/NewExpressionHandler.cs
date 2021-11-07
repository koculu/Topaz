using Esprima.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Interop;

namespace Tenray.Topaz.Expressions
{
    internal static partial class NewExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (NewExpression)expression;
            var callee = scriptExecutor.ExecuteExpressionAndGetValue(expr.Callee, token);
            if (callee is not ITypeProxy typeProxy)
            {
                Exceptions.ThrowCanNotCallConstructor(callee);
                return null;
            }
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
            return typeProxy.CallConstructor(args);
        }
    }
}

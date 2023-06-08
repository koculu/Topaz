using Esprima.Ast;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements;

internal static partial class ForStatementHandler
{
    internal static object Execute(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
    {
        var expr = (ForStatement)statement;
        var body = expr.Body;
        var init = expr.Init;
        var test = expr.Test;
        var update = expr.Update;

        // outer for loop scope is required for let and const variables not defined after for loop.
        scriptExecutor = scriptExecutor.NewBlockScope();
        scriptExecutor.ExecuteStatement(init, token);
        if (body is not BlockStatement blockBody)
        {
            while (JavascriptTypeUtility
              .IsObjectTrue(scriptExecutor.ExecuteExpressionAndGetValue(test, token)))
            {
                token.ThrowIfCancellationRequested();
                var result = scriptExecutor.ExecuteStatement(body, token);
                if (result is ReturnWrapper)
                {
                    scriptExecutor.ReturnToPool();
                    return result;
                }
                if (result is BreakWrapper)
                    break;
                scriptExecutor.ExecuteStatement(update, token);
            }
            var returnValue2 = scriptExecutor.GetNullOrUndefined();
            scriptExecutor.ReturnToPool();
            return returnValue2;
        }

        var list = blockBody.Body;
        var len = list.Count;
        while (JavascriptTypeUtility
            .IsObjectTrue(scriptExecutor.ExecuteExpressionAndGetValue(test, token)))
        {
            token.ThrowIfCancellationRequested();

            // inner block scope is required for let and const variables
            // are allowed to be defined multiple times in the loop.
            var bodyScope = scriptExecutor.NewBlockScope();
            var breaked = false;
            for (var i = 0; i < len; ++i)
            {
                var result = bodyScope.ExecuteStatement(list[i], token);
                if (result is ContinueWrapper)
                {
                    break;
                }
                else if (result is BreakWrapper)
                {
                    breaked = true;
                    break;
                }
                else if (result is ReturnWrapper)
                {
                    bodyScope.ReturnToPool();
                    scriptExecutor.ReturnToPool();
                    return result;
                }
            }
            bodyScope.ReturnToPool();
            if (breaked) break;
            scriptExecutor.ExecuteStatement(update, token);
        }
        var returnValue = scriptExecutor.GetNullOrUndefined();
        scriptExecutor.ReturnToPool();
        return returnValue;
    }
}

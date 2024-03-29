﻿using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements;

internal static partial class ForStatementHandler
{
    internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
    {
        var expr = (ForStatement)statement;
        var body = expr.Body;
        var init = expr.Init;
        var test = expr.Test;
        var update = expr.Update;
        scriptExecutor = scriptExecutor.NewBlockScope();
        await scriptExecutor.ExecuteStatementAsync(init, token);
        if (body is not BlockStatement blockBody)
        {
            while (JavascriptTypeUtility
              .IsObjectTrue(await 
              scriptExecutor.ExecuteExpressionAndGetValueAsync(test, token)))
            {
                token.ThrowIfCancellationRequested();
                var result = await scriptExecutor.ExecuteStatementAsync(body, token);
                if (result is ReturnWrapper)
                {
                    scriptExecutor.ReturnToPool();
                    return result;
                }
                if (result is BreakWrapper)
                    break;
                await scriptExecutor.ExecuteStatementAsync(update, token);
            }
            var returnValue2 = scriptExecutor.GetNullOrUndefined();
            scriptExecutor.ReturnToPool();
            return returnValue2;
        }

        var list = blockBody.Body;
        var len = list.Count;
        while (JavascriptTypeUtility
            .IsObjectTrue(await 
                scriptExecutor.ExecuteExpressionAndGetValueAsync(test, token)))
        {
            token.ThrowIfCancellationRequested();
            var bodyScope = scriptExecutor.NewBlockScope();
            var breaked = false;
            for (var i = 0; i < len; ++i)
            {
                var result = await bodyScope.ExecuteStatementAsync(list[i], token);
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
            await scriptExecutor.ExecuteStatementAsync(update, token);
        }
        var returnValue = scriptExecutor.GetNullOrUndefined();
        scriptExecutor.ReturnToPool();
        return returnValue;
    }
}

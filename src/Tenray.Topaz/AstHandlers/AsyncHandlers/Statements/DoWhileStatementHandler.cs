using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class DoWhileStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
        {
            var expr = (DoWhileStatement)statement;
            var body = expr.Body;
            var test = expr.Test;
            if (body is not BlockStatement blockBody)
            {
                do
                {
                    token.ThrowIfCancellationRequested();
                    var result = await scriptExecutor.ExecuteStatementAsync(body, token);
                    if (result is ReturnWrapper)
                        return result;
                    if (result is BreakWrapper)
                        break;
                }
                while (JavascriptTypeUtility
                    .IsObjectTrue(await
                        scriptExecutor.ExecuteExpressionAndGetValueAsync(test, token)));
                return scriptExecutor.GetNullOrUndefined();
            }
            var list = blockBody.Body;
            var len = list.Count;
            do
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
                        return result;
                    }
                }
                bodyScope.ReturnToPool();
                if (breaked) break;
            }
            while (JavascriptTypeUtility
                .IsObjectTrue(await 
                    scriptExecutor.ExecuteExpressionAndGetValueAsync(test, token)));
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

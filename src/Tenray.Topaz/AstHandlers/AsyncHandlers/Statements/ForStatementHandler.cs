using Esprima.Ast;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class ForStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ForStatement)statement;
            var init = expr.Init;
            var test = expr.Test;
            var update = expr.Update;
            var blockBody = expr.Body as BlockStatement;
            scriptExecutor = scriptExecutor.NewBlockScope();
            scriptExecutor.ExecuteStatement(init);
            while (JavascriptTypeUtility
                .IsObjectTrue(await
                    scriptExecutor.ExecuteExpressionAndGetValueAsync(test)))
            {
                var bodyScope = scriptExecutor.NewBlockScope();
                if (blockBody == null)
                {
                    var result = await bodyScope.ExecuteStatementAsync(expr.Body);
                    if (result is ReturnWrapper)
                        return result;
                    if (result is BreakWrapper)
                        break;
                    await scriptExecutor.ExecuteStatementAsync(update);
                    continue;
                }
                var list = blockBody.Body;
                var len = list.Count;

                var breaked = false;
                var continued = false;
                for (var i = 0; i < len; ++i)
                {
                    var result = await bodyScope.ExecuteStatementAsync(list[i]);
                    if (result is ContinueWrapper)
                    {
                        continued = true;
                        break;
                    }
                    else if (result is BreakWrapper)
                    {
                        breaked = true;
                        break;
                    }
                    else if (result is ReturnWrapper)
                        return result;
                }
                if (breaked) break;
                await scriptExecutor.ExecuteStatementAsync(update);
                if (continued) continue;
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

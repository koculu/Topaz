using Esprima.Ast;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class DoWhileStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (DoWhileStatement)statement;
            var test = expr.Test;
            var blockBody = expr.Body as BlockStatement;
            do
            {
                var bodyScope = scriptExecutor.NewBlockScope();
                if (blockBody == null)
                {
                    var result = await bodyScope.ExecuteStatementAsync(expr.Body);
                    if (result is ReturnWrapper)
                        return result;
                    if (result is BreakWrapper)
                        break;
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
                if (continued) continue;
            }
            while (JavascriptTypeUtility
                .IsObjectTrue(
                    await scriptExecutor
                    .ExecuteExpressionAndGetValueAsync(test)
                ));
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

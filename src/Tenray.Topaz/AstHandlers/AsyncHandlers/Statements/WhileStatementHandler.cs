using Esprima.Ast;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Statements
{
    internal static partial class WhileStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (WhileStatement)statement;
            var body = expr.Body;
            var test = expr.Test;
            if (body is not BlockStatement blockBody)
            {
                while (JavascriptTypeUtility
                   .IsObjectTrue(await
                        scriptExecutor.ExecuteExpressionAndGetValueAsync(test)))
                {
                    var result = await scriptExecutor.ExecuteStatementAsync(body);
                    if (result is ReturnWrapper)
                        return result;
                    if (result is BreakWrapper)
                        break;
                    continue;
                }
                return scriptExecutor.GetNullOrUndefined();
            }

            var list = blockBody.Body;
            var len = list.Count;
            while (JavascriptTypeUtility
                .IsObjectTrue(await 
                    scriptExecutor.ExecuteExpressionAndGetValueAsync(test)))
            {
                var bodyScope = scriptExecutor.NewBlockScope();
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
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

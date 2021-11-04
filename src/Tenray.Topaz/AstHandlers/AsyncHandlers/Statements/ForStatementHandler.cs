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
            var body = expr.Body;
            var init = expr.Init;
            var test = expr.Test;
            var update = expr.Update;
            scriptExecutor = scriptExecutor.NewBlockScope();
            await scriptExecutor.ExecuteStatementAsync(init);
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
                    await scriptExecutor.ExecuteStatementAsync(update);
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
                for (var i = 0; i < len; ++i)
                {
                    var result = await bodyScope.ExecuteStatementAsync(list[i]);
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
                        return result;
                }
                if (breaked) break;
                await scriptExecutor.ExecuteStatementAsync(update);
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

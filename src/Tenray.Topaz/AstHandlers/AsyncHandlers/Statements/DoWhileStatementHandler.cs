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
            var body = expr.Body;
            var test = expr.Test;
            if (body is not BlockStatement blockBody)
            {
                do
                {
                    var result = await scriptExecutor.ExecuteStatementAsync(body);
                    if (result is ReturnWrapper)
                        return result;
                    if (result is BreakWrapper)
                        break;
                    continue;
                }
                while (JavascriptTypeUtility
                    .IsObjectTrue(await
                        scriptExecutor.ExecuteExpressionAndGetValueAsync(test)));
                return scriptExecutor.GetNullOrUndefined();
            }
            var list = blockBody.Body;
            var len = list.Count;
            do
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
            while (JavascriptTypeUtility
                .IsObjectTrue(await 
                    scriptExecutor.ExecuteExpressionAndGetValueAsync(test)));
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

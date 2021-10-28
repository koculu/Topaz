using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal class ForStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ForStatement)statement;
            var init = expr.Init;
            var test = expr.Test;
            var update = expr.Update;
            var blockBody = expr.Body as BlockStatement;
            scriptExecutor = scriptExecutor.NewBlockScope();
            scriptExecutor.ExecuteStatement(init);
            while (JavascriptTypeUtility
                .IsObjectTrue(scriptExecutor.ExecuteExpressionAndGetValue(test)))
            {
                var bodyScope = scriptExecutor.NewBlockScope();
                if (blockBody == null)
                {
                    var result = bodyScope.ExecuteStatement(expr.Body);
                    if (result is ReturnWrapper)
                        return result;
                    if (result is BreakWrapper)
                        break;
                    scriptExecutor.ExecuteStatement(update);
                    continue;
                }
                var list = blockBody.Body;
                var len = list.Count;

                var breaked = false;
                var continued = false;
                for (var i = 0; i < len; ++i)
                {
                    var result = bodyScope.ExecuteStatement(list[i]);
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
                scriptExecutor.ExecuteStatement(update);
                if (continued) continue;
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

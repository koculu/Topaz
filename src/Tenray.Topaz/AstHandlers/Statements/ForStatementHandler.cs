using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class ForStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ForStatement)statement;
            var body = expr.Body;
            var init = expr.Init;
            var test = expr.Test;
            var update = expr.Update;
            scriptExecutor = scriptExecutor.NewBlockScope();
            scriptExecutor.ExecuteStatement(init);
            if (body is not BlockStatement blockBody)
            {
                while (JavascriptTypeUtility
                  .IsObjectTrue(scriptExecutor.ExecuteExpressionAndGetValue(test)))
                {
                    var result = scriptExecutor.ExecuteStatement(body);
                    if (result is ReturnWrapper)
                        return result;
                    if (result is BreakWrapper)
                        break;
                    scriptExecutor.ExecuteStatement(update);
                    continue;
                }
                return scriptExecutor.GetNullOrUndefined();
            }

            var list = blockBody.Body;
            var len = list.Count;
            while (JavascriptTypeUtility
                .IsObjectTrue(scriptExecutor.ExecuteExpressionAndGetValue(test)))
            {
                var bodyScope = scriptExecutor.NewBlockScope();
                var breaked = false;
                for (var i = 0; i < len; ++i)
                {
                    var result = bodyScope.ExecuteStatement(list[i]);
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
                scriptExecutor.ExecuteStatement(update);
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

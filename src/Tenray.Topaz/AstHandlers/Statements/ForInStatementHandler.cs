using Esprima.Ast;
using System.Collections;
using Tenray.Topaz.Core;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Statements
{
    internal static partial class ForInStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ForInStatement)statement;
            var left = expr.Left;
            var right = expr.Right;
            var blockBody = expr.Body as BlockStatement;
            scriptExecutor = scriptExecutor.NewBlockScope();
            scriptExecutor.ExecuteStatement(left);

            var variableDeclaration = (VariableDeclaration)left;

            var rightValue = scriptExecutor.ExecuteExpressionAndGetValue(right);
            var objectKeys = DynamicHelper.GetObjectKeys(rightValue);
            foreach (var key in objectKeys)
            {
                BindingHelper.BindVariables(scriptExecutor, key, variableDeclaration);
                var bodyScope = scriptExecutor.NewBlockScope();
                if (blockBody == null)
                {
                    var result = bodyScope.ExecuteStatement(expr.Body);
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
                if (continued) continue;
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

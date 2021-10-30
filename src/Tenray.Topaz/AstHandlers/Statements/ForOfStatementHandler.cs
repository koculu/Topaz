using Esprima.Ast;
using System.Collections;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Statements
{
    internal static partial class ForOfStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ForOfStatement)statement;
            var body = expr.Body;
            var left = expr.Left;
            var right = expr.Right;
            scriptExecutor = scriptExecutor.NewBlockScope();
            scriptExecutor.ExecuteStatement(left);

            var variableDeclaration = (VariableDeclaration)left;
            var rightValue = scriptExecutor.ExecuteExpressionAndGetValue(right);
            if (rightValue is not IEnumerable enumerable)
                return Exceptions.ThrowValueIsNotEnumerable(rightValue);

            if (body is not BlockStatement blockBody)
            {
                foreach (var item in enumerable)
                {
                    BindingHelper.BindVariables(scriptExecutor, item, variableDeclaration);                    
                    var result = scriptExecutor.ExecuteStatement(body);
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
            foreach (var item in enumerable)
            {
                BindingHelper.BindVariables(scriptExecutor, item, variableDeclaration);
                var bodyScope = scriptExecutor.NewBlockScope();
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

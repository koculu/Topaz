using Esprima.Ast;
using System.Collections;
using System.Threading;
using Tenray.Topaz.Core;
using Tenray.Topaz.Interop;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Statements
{
    internal static partial class ForInStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
        {
            var expr = (ForInStatement)statement;
            var body = expr.Body;
            var left = expr.Left;
            var right = expr.Right;

            var variableDeclaration = (VariableDeclaration)left;

            var rightValue = scriptExecutor.ExecuteExpressionAndGetValue(right, token);
            var objectKeys = DynamicObjectKeysGetter.GetObjectKeys(rightValue);

            if (body is not BlockStatement blockBody)
            {
                foreach (var key in objectKeys)
                {
                    token.ThrowIfCancellationRequested();
                    var bodyScope = scriptExecutor.NewBlockScope();
                    variableDeclaration.Declarations[0].Init = new ValueWrapper(key);
                    bodyScope.ExecuteStatement(variableDeclaration, token);
                    var result = bodyScope.ExecuteStatement(body, token);
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
            foreach (var key in objectKeys)
            {
                var bodyScope = scriptExecutor.NewBlockScope();
                variableDeclaration.Declarations[0].Init = new ValueWrapper(key);
                bodyScope.ExecuteStatement(variableDeclaration, token);
                var breaked = false;
                for (var i = 0; i < len; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    var result = bodyScope.ExecuteStatement(list[i], token);
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
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

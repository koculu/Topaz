using Esprima.Ast;
using System.Collections;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.Interop;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Statements
{
    internal static partial class ForInStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ForInStatement)statement;
            var body = expr.Body;
            var left = expr.Left;
            var right = expr.Right;

            var variableDeclaration = (VariableDeclaration)left;

            var rightValue = await scriptExecutor.ExecuteExpressionAndGetValueAsync(right);
            var objectKeys = DynamicObjectKeysGetter.GetObjectKeys(rightValue);

            if (body is not BlockStatement blockBody)
            {
                foreach (var key in objectKeys)
                {
                    var bodyScope = scriptExecutor.NewBlockScope();
                    variableDeclaration.Declarations[0].Init = new ValueWrapper(key);
                    bodyScope.ExecuteStatement(variableDeclaration);
                    var result = await bodyScope.ExecuteStatementAsync(body);
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
                bodyScope.ExecuteStatement(variableDeclaration);
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
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

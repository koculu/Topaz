using Esprima.Ast;
using System;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class ExpressionStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ExpressionStatement)statement;
            return await scriptExecutor.ExecuteExpressionAndGetValueAsync(expr.Expression);
        }
    }
}

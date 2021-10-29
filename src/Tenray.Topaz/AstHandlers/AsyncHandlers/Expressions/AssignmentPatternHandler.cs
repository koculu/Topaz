using Esprima.Ast;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class AssignmentPatternHandler
    {
        internal async static ValueTask<object> ExecuteAsync(
            ScriptExecutor scriptExecutor,
            Node expression)
        {
            var expr = (AssignmentPattern)expression;
            var left = await scriptExecutor.ExecuteStatementAsync(expr.Left);
            var right = await scriptExecutor.ExecuteExpressionAndGetValueAsync(expr.Right);
            scriptExecutor
                .SetReferenceValue(left, right);
            return right;
        }
    }
}

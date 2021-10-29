using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class AssignmentPatternHandler
    {
        internal static object Execute(
            ScriptExecutor scriptExecutor,
            Node expression)
        {
            var expr = (AssignmentPattern)expression;
            var left = scriptExecutor.ExecuteStatement(expr.Left);
            var right = scriptExecutor.ExecuteExpressionAndGetValue(expr.Right);
            scriptExecutor
                .SetReferenceValue(left, right);
            return right;
        }
    }
}

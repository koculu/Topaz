using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class ChainExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (ChainExpression)expression;
            // Possible values:
            // CallExpression | ComputedMemberExpression | StaticMemberExpression
            return scriptExecutor.ExecuteStatement(expr.Expression);
        }
    }


}

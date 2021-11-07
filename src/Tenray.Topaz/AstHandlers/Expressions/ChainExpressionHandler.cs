using Esprima.Ast;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class ChainExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (ChainExpression)expression;
            // Possible values:
            // CallExpression | ComputedMemberExpression | StaticMemberExpression
            return scriptExecutor.ExecuteStatement(expr.Expression, token);
        }
    }


}

using Esprima.Ast;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class MemberExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (MemberExpression)expression;
            var obj = await scriptExecutor.ExecuteStatementAsync(expr.Object);
            var prop = await scriptExecutor.ExecuteStatementAsync(expr.Property);
            return new TopazMemberAccessor(obj, prop, expr.Computed, expr.Optional);
        }
    }
}

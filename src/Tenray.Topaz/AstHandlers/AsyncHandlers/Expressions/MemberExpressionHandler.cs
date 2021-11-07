using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class MemberExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (MemberExpression)expression;
            var obj = await scriptExecutor.ExecuteStatementAsync(expr.Object, token);
            var prop = await scriptExecutor.ExecuteStatementAsync(expr.Property, token);
            return new TopazMemberAccessor(obj, prop, expr.Computed, expr.Optional);
        }
    }
}

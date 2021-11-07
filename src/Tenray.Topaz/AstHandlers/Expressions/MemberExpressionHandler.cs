using Esprima.Ast;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class MemberExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (MemberExpression)expression;
            var obj = scriptExecutor.ExecuteStatement(expr.Object, token);
            var prop = scriptExecutor.ExecuteStatement(expr.Property, token);
            return new TopazMemberAccessor(obj, prop, expr.Computed, expr.Optional);
        }
    }
}

using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal class MemberExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (MemberExpression)expression;
            var obj = scriptExecutor.ExecuteStatement(expr.Object);
            var prop = scriptExecutor.ExecuteStatement(expr.Property);
            return new TopazMemberAccessor(obj, prop, expr.Computed, expr.Optional);
        }
    }
}

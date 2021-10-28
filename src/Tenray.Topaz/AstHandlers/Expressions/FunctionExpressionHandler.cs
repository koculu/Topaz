using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal class FunctionExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (FunctionExpression)expression;
            var name = expr.Id?.Name ?? string.Empty;
            var function = new TopazFunction(
                scriptExecutor.NewFunctionScope(),
                name,
                expr);
            if (!string.IsNullOrWhiteSpace(name))
            {
                scriptExecutor.DefineVariable(name, function, VariableKind.Var);
            }
            return function;
        }
    }
}

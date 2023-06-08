using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class FunctionExpressionHandler
{
    internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
    {
        var expr = (FunctionExpression)expression;
        var identifier = expr.Id;
        var name = identifier?.Name ?? string.Empty;
        var function = new TopazFunction(
            scriptExecutor.NewFunctionScope(),
            name,
            expr);
        scriptExecutor.DefineVariable(identifier, function, VariableKind.Var);
        return function;
    }
}

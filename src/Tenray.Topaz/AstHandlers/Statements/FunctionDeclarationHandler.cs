using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements;

internal static partial class FunctionDeclarationHandler
{
    internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
    {
        var expr = (FunctionDeclaration)statement;
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

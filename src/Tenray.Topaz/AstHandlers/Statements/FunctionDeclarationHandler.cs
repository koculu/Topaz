using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class FunctionDeclarationHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (FunctionDeclaration)statement;
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

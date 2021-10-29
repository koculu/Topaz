using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class IdentifierHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (Identifier)expression;
            return new TopazIdentifier(scriptExecutor, expr.Name);
        }
    }
    
}

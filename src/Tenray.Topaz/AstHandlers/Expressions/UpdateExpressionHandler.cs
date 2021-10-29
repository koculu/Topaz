using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class UpdateExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            return UnaryExpressionHandler.Execute(scriptExecutor, expression);
        }
    }
}

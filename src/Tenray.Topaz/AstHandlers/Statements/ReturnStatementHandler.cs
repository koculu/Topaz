using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal class ReturnStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ReturnStatement)statement;
            return new ReturnWrapper(scriptExecutor.ExecuteStatement(expr.Argument));
        }
    }
}

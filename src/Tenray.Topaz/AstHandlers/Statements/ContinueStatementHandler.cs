using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal class ContinueStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            return ContinueWrapper.Instance;
        }
    }
}

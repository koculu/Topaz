using Esprima.Ast;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class ContinueStatementHandler
    {
        internal static object ExecuteAsync(ScriptExecutor scriptExecutor, Node statement)
        {
            return ContinueWrapper.Instance;
        }
    }
}

using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class BlockStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
        {
            var expr = (BlockStatement)statement;
            var list = expr.Body;
            var len = list.Count;
            scriptExecutor = scriptExecutor.NewBlockScope();
            for (var i = 0; i < len; ++i)
            {
                var el = list[i];
                var result = await scriptExecutor.ExecuteStatementAsync(el, token);
                if (result is ReturnWrapper ||
                    result is BreakWrapper ||
                    result is ContinueWrapper)
                    return result;
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }

}

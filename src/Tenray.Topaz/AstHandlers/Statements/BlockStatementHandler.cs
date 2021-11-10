using Esprima.Ast;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class BlockStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
        {
            var expr = (BlockStatement)statement;
            var list = expr.Body;
            var len = list.Count;
            scriptExecutor = scriptExecutor.NewBlockScope();
            for (var i = 0; i < len; ++i)
            {
                var el = list[i];
                var result = scriptExecutor.ExecuteStatement(el, token);
                if (result is ReturnWrapper ||
                    result is BreakWrapper ||
                    result is ContinueWrapper)
                {
                    scriptExecutor.ReturnToPool();
                    return result;
                }
            }
            var returnValue = scriptExecutor.GetNullOrUndefined();
            scriptExecutor.ReturnToPool();
            return returnValue;
        }
    }

}

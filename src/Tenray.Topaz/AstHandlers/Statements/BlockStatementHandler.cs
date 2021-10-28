using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal class BlockStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (BlockStatement)statement;
            var list = expr.Body;
            var len = list.Count;
            scriptExecutor = scriptExecutor.NewBlockScope();
            for (var i = 0; i < len; ++i)
            {
                var el = list[i];
                var result = scriptExecutor.ExecuteStatement(el);
                if (result is ReturnWrapper ||
                    result is BreakWrapper ||
                    result is ContinueWrapper)
                    return result;
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }

}

using Esprima.Ast;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class SequenceExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (SequenceExpression)expression;
            var list = expr.Expressions;
            var len = list.Count;
            object result = null;
            for (var i = 0; i < len; ++i)
            {
                result = await scriptExecutor.ExecuteStatementAsync(list[i]);
                if (result is ReturnWrapper)
                    return result;
            }
            return result;
        }
    }
}

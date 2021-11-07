using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class SequenceExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (SequenceExpression)expression;
            var list = expr.Expressions;
            var len = list.Count;
            object result = null;
            for (var i = 0; i < len; ++i)
            {
                token.ThrowIfCancellationRequested();
                result = await scriptExecutor.ExecuteStatementAsync(list[i], token);
                if (result is ReturnWrapper)
                    return result;
            }
            return result;
        }
    }
}

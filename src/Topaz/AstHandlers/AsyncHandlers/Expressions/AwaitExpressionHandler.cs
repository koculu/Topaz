using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class AwaitExpressionHandler
{
    internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
    {
        var expr = (AwaitExpression)expression;
        var result = await scriptExecutor.ExecuteStatementAsync(expr.Argument, token);
        if (result == null)
            return null;
        if (result is Task || result is ValueTask)
        {
            dynamic awaitable = result;
            if (result.GetType().IsGenericType)
                return await awaitable;
            await awaitable;
        }
        return result;
    }
}

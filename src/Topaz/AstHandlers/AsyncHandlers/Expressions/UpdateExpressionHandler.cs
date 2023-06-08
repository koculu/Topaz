using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class UpdateExpressionHandler
{
    internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
    {
        return await UnaryExpressionHandler.ExecuteAsync(scriptExecutor, expression, token);
    }
}

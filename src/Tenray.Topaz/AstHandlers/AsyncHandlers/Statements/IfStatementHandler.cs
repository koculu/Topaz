using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements;

internal static partial class IfStatementHandler
{
    internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
    {
        var expr = (IfStatement)statement;
        var test = expr.Test;
        var onTrue = expr.Consequent;
        var onFalse = expr.Alternate;
        if (JavascriptTypeUtility
            .IsObjectTrue(await
                scriptExecutor.ExecuteExpressionAndGetValueAsync(test, token)))
            return await scriptExecutor.ExecuteStatementAsync(onTrue, token);
        return await scriptExecutor.ExecuteStatementAsync(onFalse, token);
    }
}

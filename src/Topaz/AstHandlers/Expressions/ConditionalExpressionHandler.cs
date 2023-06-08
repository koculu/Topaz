using Esprima.Ast;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class ConditionalExpressionHandler
{
    internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
    {
        var expr = (ConditionalExpression)expression;
        var test = expr.Test;
        var onTrue = expr.Consequent;
        var onFalse = expr.Alternate;
        if (JavascriptTypeUtility
            .IsObjectTrue(scriptExecutor.ExecuteExpressionAndGetValue(test, token)))
            return scriptExecutor.ExecuteStatement(onTrue, token);
        return scriptExecutor.ExecuteStatement(onFalse, token);
    }
}

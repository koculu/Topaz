using Esprima.Ast;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class ConditionalExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (ConditionalExpression)expression;
            var test = expr.Test;
            var onTrue = expr.Consequent;
            var onFalse = expr.Alternate;
            if (JavascriptTypeUtility
                .IsObjectTrue(await scriptExecutor.ExecuteExpressionAndGetValueAsync(test)))
                return await scriptExecutor.ExecuteStatementAsync(onTrue);
            return await scriptExecutor.ExecuteStatementAsync(onFalse);
        }
    }
}

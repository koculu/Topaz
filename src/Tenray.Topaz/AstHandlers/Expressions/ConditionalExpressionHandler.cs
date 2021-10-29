using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class ConditionalExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (ConditionalExpression)expression;
            var test = expr.Test;
            var onTrue = expr.Consequent;
            var onFalse = expr.Alternate;
            if (JavascriptTypeUtility
                .IsObjectTrue(scriptExecutor.ExecuteExpressionAndGetValue(test)))
                return scriptExecutor.ExecuteStatement(onTrue);
            return scriptExecutor.ExecuteStatement(onFalse);
        }
    }
}

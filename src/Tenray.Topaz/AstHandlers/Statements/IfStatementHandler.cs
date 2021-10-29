using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class IfStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (IfStatement)statement;
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

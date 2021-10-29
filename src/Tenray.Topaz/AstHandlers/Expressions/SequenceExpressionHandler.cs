using Esprima.Ast;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class SequenceExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (SequenceExpression)expression;
            var list = expr.Expressions;
            var len = list.Count;
            object result = null;
            for (var i = 0; i < len; ++i)
            {
                result = scriptExecutor.ExecuteStatement(list[i]);
                if (result is ReturnWrapper)
                    return result;
            }
            return result;
        }
    }
}

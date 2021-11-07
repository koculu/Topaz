using Esprima.Ast;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class SequenceExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (SequenceExpression)expression;
            var list = expr.Expressions;
            var len = list.Count;
            object result = null;
            for (var i = 0; i < len; ++i)
            {
                token.ThrowIfCancellationRequested();
                result = scriptExecutor.ExecuteStatement(list[i], token);
                if (result is ReturnWrapper)
                    return result;
            }
            return result;
        }
    }
}

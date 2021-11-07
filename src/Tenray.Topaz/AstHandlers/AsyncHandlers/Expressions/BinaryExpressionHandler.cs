using Esprima.Ast;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions
{
    internal static partial class BinaryExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (BinaryExpression)expression;
            var left = await scriptExecutor.ExecuteExpressionAndGetValueAsync(expr.Left, token);
            var right = await scriptExecutor .ExecuteExpressionAndGetValueAsync(expr.Right, token);
            return ExecuteBinaryOperator(scriptExecutor, expr.Operator, left, right);
        }
    }
}

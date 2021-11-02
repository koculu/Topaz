using Esprima.Ast;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions
{
    internal static partial class BinaryExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (BinaryExpression)expression;
            var left = await scriptExecutor.ExecuteExpressionAndGetValueAsync(expr.Left);
            var right = await scriptExecutor .ExecuteExpressionAndGetValueAsync(expr.Right);
            var @operator = expr.Operator;

            // Try to avoid slow dynamic operator execution by type casting
            if (left is double d1)
            {
                if (right is double p1)
                    return ExecuteBinaryOperatorDouble(@operator, d1, p1);
                if (right is long p2)
                    return ExecuteBinaryOperatorDouble(@operator, d1, p2);
                if (right is int p3)
                    return ExecuteBinaryOperatorDouble(@operator, d1, p3);
            }
            else if (left is long d2)
            {
                if (right is double p1)
                    return ExecuteBinaryOperatorDouble(@operator, d2, p1);

                var convert =
                    scriptExecutor.Options.NumbersAreConvertedToDoubleInArithmeticOperations;
                if (right is long p2)
                    return
                        convert ?
                        ExecuteBinaryOperatorDouble(@operator, d2, p2) : 
                        ExecuteBinaryOperatorLong(@operator, d2, p2);
                if (right is int p3)
                    return ExecuteBinaryOperatorLong(@operator, d2, p3);
            }
            else if (left is int d3)
            {
                if (right is double p1)
                    return ExecuteBinaryOperatorDouble(@operator, d3, p1);

                var convert =
                    scriptExecutor.Options.NumbersAreConvertedToDoubleInArithmeticOperations;
                if (right is long p2)
                    return convert ?
                        ExecuteBinaryOperatorDouble(@operator, d3, p2) :
                        ExecuteBinaryOperatorLong(@operator, d3, p2);
                if (right is int p3)
                    return
                        convert ?
                        ExecuteBinaryOperatorDouble(@operator, d3, p3) : 
                        ExecuteBinaryOperatorInt(@operator, d3, p3);
            }

            if (left is Undefined)
                left = null;
            if (right is Undefined)
                right = null;
            return ExecuteBinaryOperator(scriptExecutor, expr.Operator, left, right);
        }
    }
}

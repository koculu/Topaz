using Esprima.Ast;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class AssignmentExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(
            ScriptExecutor scriptExecutor,
            Node expression, CancellationToken token)
        {
            var expr = (AssignmentExpression)expression;
            var left = expr.Left;
            var right = expr.Right;
            var @operator = expr.Operator;

            object value = await scriptExecutor.ExecuteExpressionAndGetValueAsync(right, token);
            if (left is ArrayPattern arrayPattern)
            {
                return await ArrayPatternHandler.ProcessArrayPatternAsync(
                    scriptExecutor,
                    arrayPattern,
                    value,
                    async (x, y, token) =>
                    {
                        await ProcessAssignmentAsync(scriptExecutor, @operator, x, y, token);
                    },
                    token);
            }
            else if (left is ObjectPattern objectPattern)
            {
                return await ObjectPatternHandler.ProcessObjectPatternAsync(
                    scriptExecutor,
                    objectPattern,
                    value,
                    async (x, y, token) =>
                    {
                        await ProcessAssignmentAsync(scriptExecutor, @operator, x, y, token);
                    },
                    token);
            }

            return await ProcessAssignmentAsync(scriptExecutor, expr.Operator, left, value, token);
        }

        private async static ValueTask<object> ProcessAssignmentAsync(
            ScriptExecutor scriptExecutor,
            AssignmentOperator @operator,
            object left,
            object right,
            CancellationToken token)
        {
            var scope = scriptExecutor;
            var reference = left is Expression leftExpr ?
                await scriptExecutor.ExecuteStatementAsync(leftExpr, token) :
                left;
            if (@operator == AssignmentOperator.Assign)
            {
                scope.SetReferenceValue(reference, right);
                return right;
            }
            var referenceValue = scriptExecutor.GetValue(reference);
            if (@operator == AssignmentOperator.AndAssign)
            {
                if (JavascriptTypeUtility.IsObjectTrue(referenceValue))
                    scope.SetReferenceValue(reference, right);
                return right;
            }

            if (@operator == AssignmentOperator.OrAssign)
            {
                if (JavascriptTypeUtility.IsObjectFalse(referenceValue))
                    scope.SetReferenceValue(reference, right);
                return right;
            }
            var binaryOperator = GetBinaryOperator(@operator);
            right = BinaryExpressionHandler.ExecuteBinaryOperator(scriptExecutor, binaryOperator, referenceValue, right);
            scriptExecutor.SetReferenceValue(reference, right);
            return right;
        }
    }
}

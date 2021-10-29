using Esprima.Ast;
using System;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class UnaryExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var unaryExpr = (UnaryExpression)expression;
            var expr = scriptExecutor.ExecuteStatement(unaryExpr.Argument);
            var value = scriptExecutor.GetValue(expr);
            var unaryOperator = unaryExpr.Operator;
            if (unaryOperator == UnaryOperator.Delete)
            {
                scriptExecutor.SetReferenceValue(expr, scriptExecutor.GetNullOrUndefined());
                return scriptExecutor.GetNullOrUndefined();
            }
            var newValue = ExecuteUnaryOperator(scriptExecutor, unaryOperator, value);
            if (unaryOperator == UnaryOperator.Increment ||
                unaryOperator == UnaryOperator.Decrement ||
                unaryOperator == UnaryOperator.Delete)
                scriptExecutor.SetReferenceValue(expr, newValue);
            return newValue;
        }

        private static object ExecuteUnaryOperator(
            ScriptExecutor scriptExecutor,
            UnaryOperator unaryOperator,
            dynamic value)
        {
            if (unaryOperator == UnaryOperator.TypeOf)
            {
                if (value is Undefined)
                    return value.ToString();
                return value?.GetType();
            }
            if (unaryOperator == UnaryOperator.Void)
                return scriptExecutor.GetNullOrUndefined();
            if (unaryOperator == UnaryOperator.LogicalNot)
            {
                return !JavascriptTypeUtility.IsObjectTrue(value);
            }
            
            // Try to avoid dynamic calls for known types
            if (value is double d1)
            {
                return ProcessUnaryOpDouble(unaryOperator, d1);
            }
            else if (value is long d2)
            {
                return unaryOperator switch
                {
                    UnaryOperator.Plus => d2,
                    UnaryOperator.Minus => -d2,
                    UnaryOperator.BitwiseNot => ~(long)d2,
                    UnaryOperator.Increment => ++d2,
                    UnaryOperator.Decrement => --d2,
                    _ => null,
                };
            }
            else if (value is int d3)
            {
                return unaryOperator switch
                {
                    UnaryOperator.Plus => d3,
                    UnaryOperator.Minus => -d3,
                    UnaryOperator.BitwiseNot => ~d3,
                    UnaryOperator.Increment => ++d3,
                    UnaryOperator.Decrement => --d3,
                    _ => null,
                };
            }

            var isNumeric = JavascriptTypeUtility.IsNumeric(value);
            if (value is bool)
            {
                value = value ? 1 : 0;
                isNumeric = true;
            }
            if (!isNumeric)
                return double.NaN;

            return unaryOperator switch
            {
                UnaryOperator.Plus => value,
                UnaryOperator.Minus => -value,
                UnaryOperator.BitwiseNot => ~(long)value,
                UnaryOperator.Increment => ++value,
                UnaryOperator.Decrement => --value,
                _ => null,
            };
        }

        private static object ProcessUnaryOpDouble(UnaryOperator unaryOperator, double d1)
        {
            return unaryOperator switch
            {
                UnaryOperator.Plus => d1,
                UnaryOperator.Minus => -d1,
                UnaryOperator.BitwiseNot => ~(long)d1,
                UnaryOperator.Increment => ++d1,
                UnaryOperator.Decrement => --d1,
                _ => null,
            };
        }

        private static object ProcessUnaryOpLong(UnaryOperator unaryOperator, long d1)
        {
            try
            {
                checked
                {
                    return unaryOperator switch
                    {
                        UnaryOperator.Plus => d1,
                        UnaryOperator.Minus => -d1,
                        UnaryOperator.BitwiseNot => ~(long)d1,
                        UnaryOperator.Increment => ++d1,
                        UnaryOperator.Decrement => --d1,
                        _ => null,
                    };
                }
            }
            catch (OverflowException)
            {
                return ProcessUnaryOpDouble(unaryOperator, d1);
            }
        }

        private static object ProcessUnaryOpInt(UnaryOperator unaryOperator, int d1)
        {
            try
            {
                checked
                {
                    return unaryOperator switch
                    {
                        UnaryOperator.Plus => d1,
                        UnaryOperator.Minus => -d1,
                        UnaryOperator.BitwiseNot => ~(long)d1,
                        UnaryOperator.Increment => ++d1,
                        UnaryOperator.Decrement => --d1,
                        _ => null,
                    };
                }
            }
            catch (OverflowException)
            {
                return ProcessUnaryOpLong(unaryOperator, d1);
            }
        }
    }
}

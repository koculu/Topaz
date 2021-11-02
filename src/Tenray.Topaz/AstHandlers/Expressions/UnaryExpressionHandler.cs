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
            object value)
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
                var convert =
                    scriptExecutor.Options.NumbersAreConvertedToDoubleInArithmeticOperations;
                return convert ?
                    ProcessUnaryOpDouble(unaryOperator, d2) :
                    ProcessUnaryOpLong(unaryOperator, d2);
            }
            else if (value is int d3)
            {

                var convert =
                    scriptExecutor.Options.NumbersAreConvertedToDoubleInArithmeticOperations;
                return convert ?
                    ProcessUnaryOpDouble(unaryOperator, d3) :
                    ProcessUnaryOpInt(unaryOperator, d3);
            }

            var isNumeric = JavascriptTypeUtility.IsNumeric(value);
            if (value is bool b)
            {
                value = b ? 1 : 0;
                isNumeric = true;
            }
            if (!isNumeric)
                return double.NaN;

            dynamic dynValue = value;
            return unaryOperator switch
            {
                UnaryOperator.Plus => dynValue,
                UnaryOperator.Minus => -dynValue,
                UnaryOperator.BitwiseNot => ~(long)dynValue,
                UnaryOperator.Increment => ++dynValue,
                UnaryOperator.Decrement => --dynValue,
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
                        UnaryOperator.BitwiseNot => ~d1,
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
                        UnaryOperator.BitwiseNot => ~d1,
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

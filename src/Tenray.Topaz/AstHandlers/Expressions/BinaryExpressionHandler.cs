using Esprima.Ast;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions
{
    internal static partial class BinaryExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (BinaryExpression)expression;
            var left = scriptExecutor.ExecuteExpressionAndGetValue(expr.Left, token);
            var right = scriptExecutor.ExecuteExpressionAndGetValue(expr.Right, token);
            return ExecuteBinaryOperator(scriptExecutor, expr.Operator, left, right);
        }

        private static object ExecuteBinaryOperatorDouble(
            BinaryOperator binaryOperator, 
            double d1, double d2)
        {
            return binaryOperator switch
            {
                BinaryOperator.Plus => d1 + d2,
                BinaryOperator.Minus => d1 - d2,
                BinaryOperator.Times => d1 * d2,
                BinaryOperator.Divide => d1 / d2,
                BinaryOperator.Modulo => d1 % d2,
                BinaryOperator.Equal => d1 == d2,
                BinaryOperator.NotEqual => d1 != d2,
                BinaryOperator.Greater => d1 > d2,
                BinaryOperator.GreaterOrEqual => d1 >= d2,
                BinaryOperator.Less => d1 < d2,
                BinaryOperator.LessOrEqual => d1 <= d2,
                BinaryOperator.StrictlyEqual => d1 == d2,
                BinaryOperator.StricltyNotEqual => d1 != d2,
                BinaryOperator.BitwiseAnd => (long)d1 & (long)d2,
                BinaryOperator.BitwiseOr => (long)d1 | (long)d2,
                BinaryOperator.BitwiseXOr => (long)d1 ^ (long)d2,
                BinaryOperator.LeftShift => (long)d1 << (int)d2,
                BinaryOperator.RightShift => (long)d1 >> (int)d2,
                BinaryOperator.UnsignedRightShift => (long)(((ulong)d1) >> (int)d2),
                BinaryOperator.InstanceOf => Exceptions.ThrowRightHandSideOfInstanceOfIsNotObject(),
                BinaryOperator.In => Exceptions.ThrowCannotUseInOperatorToSearchForIn(d1, d2),
                BinaryOperator.LogicalAnd =>
                    (JavascriptTypeUtility.IsObjectTrue(d1)
                    &&
                    JavascriptTypeUtility.IsObjectTrue(d2)) ? d2 : d1,
                BinaryOperator.LogicalOr =>
                    JavascriptTypeUtility.IsObjectTrue(d1) ? d1 : d2,
                BinaryOperator.Exponentiation => Math.Pow(d1, d2),
                BinaryOperator.NullishCoalescing => d1,
                _ => null
            };
        }
        
        private static object ExecuteBinaryOperatorLong(
            BinaryOperator binaryOperator, 
            long d1, long d2)
        {
            try
            {
                checked
                {
                    return binaryOperator switch
                    {
                        BinaryOperator.Plus => d1 + d2,
                        BinaryOperator.Minus => d1 - d2,
                        BinaryOperator.Times => d1 * d2,
                        BinaryOperator.Divide => (double)d1 / d2,
                        BinaryOperator.Modulo => d1 % d2,
                        BinaryOperator.Equal => d1 == d2,
                        BinaryOperator.NotEqual => d1 != d2,
                        BinaryOperator.Greater => d1 > d2,
                        BinaryOperator.GreaterOrEqual => d1 >= d2,
                        BinaryOperator.Less => d1 < d2,
                        BinaryOperator.LessOrEqual => d1 <= d2,
                        BinaryOperator.StrictlyEqual => d1 == d2,
                        BinaryOperator.StricltyNotEqual => d1 != d2,
                        BinaryOperator.BitwiseAnd => d1 & d2,
                        BinaryOperator.BitwiseOr => d1 | d2,
                        BinaryOperator.BitwiseXOr => d1 ^ d2,
                        BinaryOperator.LeftShift => d1 << (int)d2,
                        BinaryOperator.RightShift => d1 >> (int)d2,
                        BinaryOperator.UnsignedRightShift => (long)(((ulong)d1) >> (int)d2),
                        BinaryOperator.InstanceOf => Exceptions.ThrowRightHandSideOfInstanceOfIsNotObject(),
                        BinaryOperator.In => Exceptions.ThrowCannotUseInOperatorToSearchForIn(d1, d2),
                        BinaryOperator.LogicalAnd =>
                            (JavascriptTypeUtility.IsObjectTrue(d1)
                            &&
                            JavascriptTypeUtility.IsObjectTrue(d2)) ? d2 : d1,
                        BinaryOperator.LogicalOr =>
                            JavascriptTypeUtility.IsObjectTrue(d1) ? d1 : d2,
                        BinaryOperator.Exponentiation => Math.Pow(d1, d2),
                        BinaryOperator.NullishCoalescing => d1,
                        _ => null
                    };
                }
            }
            catch (OverflowException)
            {
                return ExecuteBinaryOperatorDouble(binaryOperator, d1, d2);
            }
        }

        private static object ExecuteBinaryOperatorInt(
            BinaryOperator binaryOperator, 
            int d1, int d2)
        {
            try
            {
                checked
                {
                    return binaryOperator switch
                    {
                        BinaryOperator.Plus => d1 + d2,
                        BinaryOperator.Minus => d1 - d2,
                        BinaryOperator.Times => d1 * d2,
                        BinaryOperator.Divide => (double)d1 / d2,
                        BinaryOperator.Modulo => d1 % d2,
                        BinaryOperator.Equal => d1 == d2,
                        BinaryOperator.NotEqual => d1 != d2,
                        BinaryOperator.Greater => d1 > d2,
                        BinaryOperator.GreaterOrEqual => d1 >= d2,
                        BinaryOperator.Less => d1 < d2,
                        BinaryOperator.LessOrEqual => d1 <= d2,
                        BinaryOperator.StrictlyEqual => d1 == d2,
                        BinaryOperator.StricltyNotEqual => d1 != d2,
                        BinaryOperator.BitwiseAnd => d1 & d2,
                        BinaryOperator.BitwiseOr => d1 | d2,
                        BinaryOperator.BitwiseXOr => d1 ^ d2,
                        BinaryOperator.LeftShift => d1 << d2,
                        BinaryOperator.RightShift => d1 >> d2,
                        BinaryOperator.UnsignedRightShift => (int)(((uint)d1) >> d2),
                        BinaryOperator.InstanceOf => Exceptions.ThrowRightHandSideOfInstanceOfIsNotObject(),
                        BinaryOperator.In => Exceptions.ThrowCannotUseInOperatorToSearchForIn(d1, d2),
                        BinaryOperator.LogicalAnd =>
                            (JavascriptTypeUtility.IsObjectTrue(d1)
                            &&
                            JavascriptTypeUtility.IsObjectTrue(d2)) ? d2 : d1,
                        BinaryOperator.LogicalOr =>
                            JavascriptTypeUtility.IsObjectTrue(d1) ? d1 : d2,
                        BinaryOperator.Exponentiation => Math.Pow(d1, d2),
                        BinaryOperator.NullishCoalescing => d1,
                        _ => null
                    };
                }
            }
            catch (OverflowException)
            {
                return ExecuteBinaryOperatorLong(binaryOperator, d1, d2);
            }
        }

        private static object ExecuteBinaryOperatorString(BinaryOperator binaryOperator, object d1, string d2)
        {
            return binaryOperator switch
            {
                BinaryOperator.Plus => d1 + d2,
                BinaryOperator.Minus => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1,d2),
                BinaryOperator.Times => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.Divide => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.Modulo => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.Equal => string.CompareOrdinal(d1?.ToString(), d2) == 0,
                BinaryOperator.NotEqual => string.CompareOrdinal(d1?.ToString(), d2) != 0,
                BinaryOperator.Greater => string.CompareOrdinal(d1?.ToString(), d2) > 0,
                BinaryOperator.GreaterOrEqual => string.CompareOrdinal(d1?.ToString(), d2) >= 0,
                BinaryOperator.Less => string.CompareOrdinal(d1?.ToString(), d2) < 0,
                BinaryOperator.LessOrEqual => string.CompareOrdinal(d1?.ToString(), d2) <= 0,
                BinaryOperator.StrictlyEqual => Equals(d1, d2),
                BinaryOperator.StricltyNotEqual => !Equals(d1, d2),
                BinaryOperator.BitwiseAnd => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.BitwiseOr => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.BitwiseXOr => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.LeftShift => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.RightShift => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.UnsignedRightShift => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.InstanceOf => d1?.GetType().FullName == d2,
                BinaryOperator.In => Exceptions.ThrowCannotUseInOperatorToSearchForIn(d1, d2),
                BinaryOperator.LogicalAnd =>
                    (JavascriptTypeUtility.IsObjectTrue(d1)
                    &&
                    JavascriptTypeUtility.IsObjectTrue(d2)) ? d2 : d1,
                BinaryOperator.LogicalOr =>
                    JavascriptTypeUtility.IsObjectTrue(d1) ? d1 : d2,
                BinaryOperator.Exponentiation => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.NullishCoalescing => d1 ?? d2,
                _ => null
            };
        }

        private static object ExecuteBinaryOperatorString(BinaryOperator binaryOperator, string d1, object d2)
        {
            return binaryOperator switch
            {
                BinaryOperator.Plus => d1 + d2,
                BinaryOperator.Minus => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.Times => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.Divide => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.Modulo => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.Equal => string.CompareOrdinal(d1, d2?.ToString()) == 0,
                BinaryOperator.NotEqual => string.CompareOrdinal(d1, d2?.ToString()) != 0,
                BinaryOperator.Greater => string.CompareOrdinal(d1, d2?.ToString()) > 0,
                BinaryOperator.GreaterOrEqual => string.CompareOrdinal(d1, d2?.ToString()) >= 0,
                BinaryOperator.Less => string.CompareOrdinal(d1, d2?.ToString()) < 0,
                BinaryOperator.LessOrEqual => string.CompareOrdinal(d1, d2?.ToString()) <= 0,
                BinaryOperator.StrictlyEqual => Equals(d1, d2),
                BinaryOperator.StricltyNotEqual => !Equals(d1, d2),
                BinaryOperator.BitwiseAnd => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.BitwiseOr => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.BitwiseXOr => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.LeftShift => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.RightShift => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.UnsignedRightShift => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.InstanceOf => d1?.GetType().FullName == d2?.ToString(),
                BinaryOperator.In => Exceptions.ThrowCannotUseInOperatorToSearchForIn(d1, d2),
                BinaryOperator.LogicalAnd =>
                    (JavascriptTypeUtility.IsObjectTrue(d1)
                    &&
                    JavascriptTypeUtility.IsObjectTrue(d2)) ? d2 : d1,
                BinaryOperator.LogicalOr =>
                    JavascriptTypeUtility.IsObjectTrue(d1) ? d1 : d2,
                BinaryOperator.Exponentiation => Exceptions.ThrowBinaryOperatorIsNotSupportedOn(binaryOperator, d1, d2),
                BinaryOperator.NullishCoalescing => d1 ?? d2,
                _ => null
            };
        }

        internal static object ExecuteBinaryOperator(
            ScriptExecutor scriptExecutor, BinaryOperator @operator,
            object left, object right)
        {
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
                    return
                        convert ?
                        ExecuteBinaryOperatorDouble(@operator, d2, p3) :
                        ExecuteBinaryOperatorLong(@operator, d2, p3);
            }
            else if (left is int d3)
            {
                if (right is double p1)
                    return ExecuteBinaryOperatorDouble(@operator, d3, p1);
                var convert =
                    scriptExecutor.Options.NumbersAreConvertedToDoubleInArithmeticOperations;
                if (right is long p2)
                    return
                        convert ?
                        ExecuteBinaryOperatorDouble(@operator, d3, p2) :
                        ExecuteBinaryOperatorLong(@operator, d3, p2);
                if (right is int p3)
                    return
                        convert ?
                        ExecuteBinaryOperatorDouble(@operator, d3, p3) :
                        ExecuteBinaryOperatorInt(@operator, d3, p3);
            }
            else if (left is string str1)
            {
                return ExecuteBinaryOperatorString(@operator,  str1, right);
            }
            else if (right is string str2)
            {
                return ExecuteBinaryOperatorString(@operator, left, str2);
            }

            var undefined = Undefined.Value;
            if (left == undefined)
                left = null;
            if (right == undefined)
                right = null;
            return ExecuteBinaryOperatorDynamic(scriptExecutor, @operator, left, right);
        }

        private static object ExecuteBinaryOperatorDynamic(
            ScriptExecutor scriptExecutor, BinaryOperator binaryOperator,
            dynamic left, dynamic right)
        {
            var allow = JavascriptTypeUtility.IsBinaryOperationAllowed(left, right);
            var allowNumeric = JavascriptTypeUtility.IsNumericBinaryOperationAllowed(left, right);
            var allowNoStringMix = JavascriptTypeUtility.IsBinaryOperationWithNoStringMixAllowed(left, right);
            return binaryOperator switch
            {
                BinaryOperator.Plus => allow ? (left ?? 0) + (right ?? 0) : 0,
                BinaryOperator.Minus => allow ? (left ?? 0) - (right ?? 0) : 0,
                BinaryOperator.Times => allowNumeric ? (left ?? 0) * (right ?? 0) : double.NaN,
                BinaryOperator.Divide => allowNumeric ? (left ?? 0) / (right ?? 0) : double.NaN,
                BinaryOperator.Modulo => allowNumeric ? (left ?? 0) % (right ?? 0) : double.NaN,
                BinaryOperator.Equal =>
                    object.Equals(left, right),
                BinaryOperator.NotEqual =>
                    !object.Equals(left, right),
                BinaryOperator.Greater => allowNoStringMix ? left > right : false,
                BinaryOperator.GreaterOrEqual => allowNoStringMix ? left >= right : false,
                BinaryOperator.Less => allowNoStringMix ? left < right : false,
                BinaryOperator.LessOrEqual => allowNoStringMix ? left <= right : false,
                BinaryOperator.StrictlyEqual => left?.GetType() == right?.GetType() && object.Equals(left, right),
                BinaryOperator.StricltyNotEqual => left?.GetType() != right?.GetType() || !object.Equals(left, right),
                BinaryOperator.BitwiseAnd => allowNumeric ? Convert.ToInt32(left ?? 0) & Convert.ToInt32(right ?? 0) : 0,
                BinaryOperator.BitwiseOr => allowNumeric ? Convert.ToInt32(left ?? 0) | Convert.ToInt32(right ?? 0) : 0,
                BinaryOperator.BitwiseXOr => allowNumeric ? Convert.ToInt32(left ?? 0) ^ Convert.ToInt32(right ?? 0) : 0,
                BinaryOperator.LeftShift => allowNumeric ? Convert.ToInt32(left ?? 0) << Convert.ToInt32(right ?? 0) : 0,
                BinaryOperator.RightShift => allowNumeric ? Convert.ToInt32(left ?? 0) >> Convert.ToInt32(right ?? 0) : 0,
                BinaryOperator.UnsignedRightShift => allowNumeric ? Convert.ToInt32(left ?? 0) >> Convert.ToInt32(right ?? 0) : 0,
                BinaryOperator.InstanceOf => left?.GetType().FullName == right?.ToString(),
                BinaryOperator.In =>
                    JavascriptTypeUtility.HasObjectMethod(right, "ContainsKey") ?
                        right.ContainsKey(left) :
                    JavascriptTypeUtility.HasObjectMethod(right, "Contains") ?
                        right.Contains(left) :
                        Exceptions.ThrowCannotUseInOperatorToSearchForIn(left, right),
                BinaryOperator.LogicalAnd =>
                    (JavascriptTypeUtility.IsObjectTrue(left)
                    &&
                    JavascriptTypeUtility.IsObjectTrue(right)) ? right : left,
                BinaryOperator.LogicalOr =>
                    JavascriptTypeUtility.IsObjectTrue(left) ? left : right,
                BinaryOperator.Exponentiation => allowNumeric ? Math.Pow(left ?? 0, right ?? 0) : 0,
                BinaryOperator.NullishCoalescing => left ?? right,
                _ => null
            };
        }
    }
}

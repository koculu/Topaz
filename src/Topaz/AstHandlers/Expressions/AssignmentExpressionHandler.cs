﻿using Esprima.Ast;
using System;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class AssignmentExpressionHandler
{
    internal static object Execute(
        ScriptExecutor scriptExecutor,
        Node expression, 
        CancellationToken token)
    {
        var expr = (AssignmentExpression)expression;
        var left = expr.Left;
        var right = expr.Right;
        var @operator = expr.Operator;

        object value = scriptExecutor.ExecuteExpressionAndGetValue(right, token);
        if (left is ArrayPattern arrayPattern)
        {
            return ArrayPatternHandler.ProcessArrayPattern(
                scriptExecutor,
                arrayPattern,
                value,
                (x, y, token) =>
                {
                    ProcessAssignment(scriptExecutor, @operator, x, y, token);
                }, token);
        }
        else if (left is ObjectPattern objectPattern)
        {
            return ObjectPatternHandler.ProcessObjectPattern(
                scriptExecutor,
                objectPattern,
                value,
                (x, y, token) =>
                {
                    ProcessAssignment(scriptExecutor, @operator, x, y, token);
                },
                token);
        }

        return ProcessAssignment(scriptExecutor, expr.Operator, left, value, token);
    }

    private static object ProcessAssignment(
        ScriptExecutor scriptExecutor,
        AssignmentOperator @operator,
        object left,
        object right,
        CancellationToken token)
    {
        var scope = scriptExecutor;
        var reference = left is Expression leftExpr ?
            scriptExecutor.ExecuteStatement(leftExpr, token) :
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

    private static BinaryOperator GetBinaryOperator(AssignmentOperator @operator)
    {
        return @operator switch
        {
            AssignmentOperator.PlusAssign => BinaryOperator.Plus,
            AssignmentOperator.MinusAssign => BinaryOperator.Minus,
            AssignmentOperator.TimesAssign => BinaryOperator.Times,
            AssignmentOperator.DivideAssign => BinaryOperator.Divide,
            AssignmentOperator.ModuloAssign => BinaryOperator.Modulo,
            AssignmentOperator.BitwiseAndAssign => BinaryOperator.BitwiseAnd,
            AssignmentOperator.BitwiseOrAssign => BinaryOperator.BitwiseOr,
            AssignmentOperator.BitwiseXOrAssign => BinaryOperator.BitwiseXOr,
            AssignmentOperator.LeftShiftAssign => BinaryOperator.LeftShift,
            AssignmentOperator.RightShiftAssign => BinaryOperator.RightShift,
            AssignmentOperator.UnsignedRightShiftAssign => BinaryOperator.UnsignedRightShift,
            AssignmentOperator.ExponentiationAssign => BinaryOperator.Exponentiation,
            AssignmentOperator.NullishAssign => BinaryOperator.NullishCoalescing,
            AssignmentOperator.Assign => throw new NotSupportedException("This was not possible."), 
            AssignmentOperator.AndAssign => throw new NotSupportedException("This was not possible."),
            AssignmentOperator.OrAssign => throw new NotSupportedException("This was not possible."),
            _ => throw new NotSupportedException("This was not possible."),
        };
    }
}

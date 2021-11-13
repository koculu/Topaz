using Esprima.Ast;
using System;
using System.Collections.Generic;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.ErrorHandling
{
    internal static class Exceptions
    {
        private static object GetArgumentString(object value)
        {
            return value == null ? "null" : value;
        }

        internal static void ThrowVariableIsAlreadyDefined(string identifier, ScriptExecutor scope)
        {
            throw new TopazException($"SyntaxError: Identifier '{GetArgumentString(identifier)}' has already been declared");
        }

        internal static void ThrowConstVariableCannotBeChanged(string identifier, ScriptExecutor scope)
        {
            throw new TopazException($"Const variable {GetArgumentString(identifier)} cannot be changed.");
        }

        internal static void ThrowCanNotCallConstructor(object callee)
        {
            throw new TopazException($"Constructor call on {GetArgumentString(callee)} is not defined.");
        }

        internal static void ThrowCanNotCallConstructorWithGivenArguments(string name, IReadOnlyList<object> args)
        {
            var argString = string.Join(", ", args);
            throw new TopazException($"{GetArgumentString(name)} constructor cannot be called with given arguments:\nnew {name}({argString})");
        }

        internal static void ThrowCanNotCallStaticMethod(object callee)
        {
            throw new TopazException($"Static method call on {GetArgumentString(callee)} is not defined.");
        }

        internal static void ThrowScopeIsReadOnly(ScriptExecutor scope, string name)
        {
            throw new TopazException($"The scope ({scope.ScopeType}) is readonly. Can not change {GetArgumentString(name)} variable on it.");
        }

        internal static void ThrowScopeIsFrozen(ScriptExecutor scope, string name)
        {
            throw new TopazException($"The scope ({scope.ScopeType}) is frozen. Can not define new {GetArgumentString(name)} variable on it.");
        }

        internal static object ThrowNotImplemented(Expression expression, ScriptExecutor scope)
        {
            throw new TopazException($"{expression} is not supported.");
        }

        internal static void ThrowMissingInitializerInDestructuringDeclaration()
        {
            throw new TopazException("SyntaxError: Missing initializer in destructuring declaration");
        }

        internal static object ThrowValueIsNotEnumerable(object rightValue)
        {
            throw new TopazException($"Type error: {GetArgumentString(rightValue)} is not iterable");
        }

        internal static void ThrowVariableNameCannotBeNullOrWhitespace()
        {
            throw new TopazException("Variable name can not be null or whitespace");
        }

        internal static void ThrowFunctionIsNotDefined(object callee, ScriptExecutor scope)
        {
            throw new TopazException($"Function {GetArgumentString(callee)} is not defined.");
        }

        internal static void ThrowReflectionSecurityException(object instance, object property)
        {
            throw new TopazException(
                    $"Security Exception: System.Reflection type members are not accessible:\n{GetArgumentString(instance)}.{GetArgumentString(property)}");
        }

        internal static void ThrowMemberAccessSecurityException(object instance, object property)
        {
            throw new TopazException(
                    $"Security Exception: Member '{GetArgumentString(property)}' of {GetArgumentString(instance)} is not accessible.");
        }

        internal static void ThrowCannotCallDelegateArgumentMismatch(IReadOnlyList<object> args)
        {
            var argString = string.Join(", ", args);            
            throw new TopazException(
                    $"Type Error: Delegate method call argument mismatch. delegate({argString})");
        }

        internal static void ThrowFunctionIsNull(object callee, ScriptExecutor scope)
        {
            throw new NullReferenceException($"Function {GetArgumentString(callee)} is null.");
        }

        internal static void ThrowCannotRetrieveMemberOfType(string name, object member)
        {
            throw new TopazException($"Can not retrieve member '{GetArgumentString(member)}' of type {name}.");
        }

        internal static void ThrowCannotRetrieveMemberOfNamespace(string name, object member)
        {
            throw new TopazException($"Can not retrieve member '{GetArgumentString(member)}' of namespace {name}.");
        }

        internal static void ThrowCannotAssignAValueToANamespaceMember(string name, object member)
        {
            throw new TopazException($"Can not assign a value to a namespace member {name}.{GetArgumentString(member)}.");
        }

        internal static void ThrowVariableIsNotDefined(string name)
        {
            throw new TopazException($"ReferenceError: {name} is not defined");
        }

        internal static object ThrowNotImplemented(Node statement, ScriptExecutor scope)
        {
            throw new TopazException($"{statement} is not supported.");
        }

        internal static void ThrowRestElementMustBeLastElement()
        {
            throw new TopazException("SyntaxError: Rest element must be last element");
        }

        internal static void ThrowCannotReadPropertiesOfNull(object member)
        {
            throw new NullReferenceException($"TypeError: Cannot read properties of null (reading '{member}')");
        }

        internal static object ThrowRightHandSideOfInstanceOfIsNotObject()
        {
            throw new TopazException("TypeError: Right-hand side of 'instanceof' is not an object");
        }

        internal static object ThrowCannotUseInOperatorToSearchForIn(object d1, object d2)
        {
            throw new TopazException($"TypeError: Cannot use 'in' operator to search for '{GetArgumentString(d1)}' in {GetArgumentString(d2)}");
        }

        internal static void ThrowReferenceIsUndefined(object instance)
        {
            throw new TopazException($"ReferenceError: {instance} is not defined");
        }

        internal static void ThrowExpectsReferenceValueOnLeftSideOfAssignment(object reference, object value, ScriptExecutor ScriptExecutor)
        {
            throw new TopazException($"Expects Reference Value On Left Side Of Assignment.");
        }

        internal static void ThrowNullReferenceException(string msg)
        {
            throw new NullReferenceException(msg);
        }

        internal static object ThrowCannotCallFunction(object value)
        {
            throw new TopazException($"Can not call {GetArgumentString(value)} as a function.");
        }

        internal static void ThrowInvalidLeftHandSideInAssignment(string reference)
        {
            throw new TopazException($"SyntaxError: Invalid left-hand side in assignment: {GetArgumentString(reference)}");
        }

        internal static void ThrowCannotDefineVariableWithGivenObject(object value)
        {
            throw new TopazException($"Internal error: Cannot define variable with given object: {GetArgumentString(value)}");
        }

        internal static void ThrowCannotFindFunctionMatchingGivenArguments(string name, IReadOnlyList<object> args)
        {
            var argString = string.Join(", ", args);
            throw new TopazException($"Interop error: Cannot find function matching given arguments:\n{name}({argString})");
        }

        internal static void ThrowReduceOfEmptyArrayWithNoInitialValue()
        {
            throw new TopazException("TypeError: Reduce of empty array with no initial value");
        }

        internal static void CannotConvertValueToTargetType(object value, Type propertyType)
        {
            throw new TopazException($"TypeError: Cannot convert value {GetArgumentString(value)} to target type '{propertyType}'.");
        }
    }
}

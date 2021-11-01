using Esprima.Ast;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Interop;
using Tenray.Topaz.Options;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Core
{
    internal partial class ScriptExecutor
    {
        public void AddOrUpdateVariableValueAndKindInTheScope(
            string name, 
            object value, 
            VariableKind kind,
            VariableState state = VariableState.None)
        {
            var scope = this;
            if (kind == VariableKind.Var &&
                Options.VarScopeBehavior == VarScopeBehavior.FunctionScope)
            {
                // Var should be defined in function scope or global scope.
                // Block statements cannot have var defined.
                while (scope.ScopeType == ScopeType.Block)
                    scope = scope.ParentScope;
            }
            if (scope.IsReadOnly)
                Exceptions.ThrowScopeIsReadOnly(scope, name);

            if (scope.IsThreadSafeScope)
            {
                if (scope.IsFrozen &&
                    !scope.SafeVariables.ContainsKey(name))
                    Exceptions.ThrowScopeIsFrozen(scope, name);

                scope.isEmptyScope = false;
                scope.SafeVariables.AddOrUpdate(name,
                    new Variable(value, kind),
                    (key, old) =>
                    {
                        old.SetValueAndKind(value, kind, state);
                        return old;
                    });                
            }
            else
            {
                if (scope.IsFrozen &&
                    !scope.UnsafeVariables.ContainsKey(name))
                    Exceptions.ThrowScopeIsFrozen(scope, name);

                scope.isEmptyScope = false;
                ref var refVar = ref scope.UnsafeVariables.GetOrAddValueRef(name);
                if (refVar == null)
                    refVar = new Variable(value, kind, state);
                else
                    refVar.SetValueAndKind(value, kind, state);
            }
        }

        public void AddOrUpdateVariableValueInTheScope(
            string name,
            object value,
            VariableKind defaultKind,
            bool? shouldCapture = null)
        {
            var scope = this;
            if (defaultKind == VariableKind.Var &&
                Options.VarScopeBehavior == VarScopeBehavior.FunctionScope)
            {
                // Var should be defined in function scope or global scope.
                // Block statements cannot have var defined.
                while (scope.ScopeType == ScopeType.Block)
                    scope = scope.ParentScope;
            }

            if (scope.IsReadOnly)
                Exceptions.ThrowScopeIsReadOnly(scope, name);

            if (scope.IsThreadSafeScope)
            {
                if (scope.IsFrozen &&
                    !scope.SafeVariables.ContainsKey(name))
                    Exceptions.ThrowScopeIsFrozen(scope, name);
                scope.isEmptyScope = false;
                scope.SafeVariables.AddOrUpdate(name,
                    new Variable(value, defaultKind)
                    {
                        ShouldCapture = shouldCapture ??
                            defaultKind == VariableKind.Let
                    },
                    (key, old) =>
                    {
                        old.Value = value;
                        if (shouldCapture.HasValue)
                            old.ShouldCapture = shouldCapture.Value;
                        return old;
                    });
            }
            else
            {
                if (scope.IsFrozen && 
                    !scope.UnsafeVariables.ContainsKey(name))
                    Exceptions.ThrowScopeIsFrozen(scope, name);

                scope.isEmptyScope = false;
                ref var refVar = ref scope.UnsafeVariables.GetOrAddValueRef(name);
                if (refVar == null)
                    refVar = new Variable(value, defaultKind);
                else
                    refVar.Value = value;

                if (shouldCapture.HasValue)
                    refVar.ShouldCapture = shouldCapture.Value;
            }
        }

        internal void SetVariableValue(string name, object value)
        {
            var scope = this;
            ScriptExecutor previousScope = null;
            while (scope != null)
            {
                if (scope.IsThreadSafeScope)
                {
                    if (scope.SafeVariables
                        .TryGetValue(name, out var variable))
                    {
                        if (scope.IsReadOnly)
                            Exceptions.ThrowScopeIsReadOnly(scope, name);
                        if (variable.Kind == VariableKind.Const)
                            Exceptions.ThrowConstVariableCannotBeChanged(name, this);
                        variable.Value = value;

                        // Closure variable assignment propagation:
                        if (!variable.IsCaptured)
                            return;
                    }
                }
                else
                {
                    if (scope.UnsafeVariables
                        .TryGetValue(name, out var variable))
                    {
                        if (scope.IsReadOnly)
                            Exceptions.ThrowScopeIsReadOnly(scope, name);
                        if (variable.Kind == VariableKind.Const)
                            Exceptions.ThrowConstVariableCannotBeChanged(name, this);
                        variable.Value = value;

                        // Closure variable assignment propagation:
                        if (!variable.IsCaptured)
                            return;
                    }
                }

                if (scope.ScopeType == ScopeType.Global)
                {
                    // We have reached global scope. This means,
                    // variable is not defined in any scope.
                    switch (scope
                        .Options
                        .AssignmentWithoutDefinitionBehavior)
                    {
                        case
                            AssignmentWithoutDefinitionBehavior
                            .DefineAsVarInExecutionScope:
                            AddOrUpdateVariableValueAndKindInTheScope(name, value, VariableKind.Var);
                            break;
                        case
                            AssignmentWithoutDefinitionBehavior
                            .DefineAsVarInGlobalScope:
                            scope.AddOrUpdateVariableValueAndKindInTheScope(name, value, VariableKind.Var);
                            break;
                        case
                            AssignmentWithoutDefinitionBehavior
                            .ThrowException:
                            Exceptions.ThrowVariableIsNotDefined(name);
                            break;
                        case
                            AssignmentWithoutDefinitionBehavior
                            .DefineAsVarInFirstChildOfGlobalScope:
                            (previousScope ?? scope).AddOrUpdateVariableValueAndKindInTheScope(name, value, VariableKind.Var);
                            break;
                        case
                            AssignmentWithoutDefinitionBehavior
                            .DefineAsLetInExecutionScope:
                            AddOrUpdateVariableValueAndKindInTheScope(name, value, VariableKind.Let);
                            break;
                    }
                }
                // Function scope variable dictionary is frozen (immutable)!
                if (scope.ScopeType != ScopeType.Function)
                    previousScope = scope;
                scope = scope.ParentScope;
            }

            return;
        }

        internal void SetReferenceValue(
            object reference,
            object value)
        {
            if (reference is TopazIdentifier identifier)
            {
                identifier.SetVariableValue(this, value);
                return;
            }

            if (reference is TopazMemberAccessor topazMemberAccessor)
            {
                if (topazMemberAccessor.Optional)
                    Exceptions.ThrowInvalidLeftHandSideInAssignment(topazMemberAccessor.ToString());
                SetMemberValue(
                    topazMemberAccessor.Instance,
                    topazMemberAccessor.Property,
                    value,
                    topazMemberAccessor.Computed);
                return;
            }
            Exceptions.ThrowExpectsReferenceValueOnLeftSideOfAssignment(reference, value, this);
        }

        internal void SetMemberValue(object instance, object property, object value, bool computed)
        {
            var obj = GetValue(instance);
            var member = GetVariableNameOrValue(property, !computed);
            
            if (obj == null)
            {
                if (Options.AllowNullReferenceMemberAccess)
                    return;
                Exceptions.ThrowCannotReadPropertiesOfNull(member);
            }

            if (obj is ITypeProxy typeProxy)
            {
                if (typeProxy
                    .TrySetStaticMember(
                    member?.ToString(),
                    member,
                    computed))
                    return;
            }

            if (obj is Undefined)
            {
                if (Options.AllowUndefinedReferenceMemberAccess)
                    return;
                Exceptions.ThrowReferenceIsUndefined(instance);
            }

            TopazEngine.TrySetObjectMember(obj, member, value, computed);
        }
    }
}

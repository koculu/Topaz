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
        internal bool TryGetVariableInTheScope(string name, out Variable variable)
        {
            var scope = this;
            if (scope.IsThreadSafeScope)
            {
                if (scope.SafeVariables.TryGetValue(name, out variable))
                    return true;
            }
            else
            {
                if (scope.UnsafeVariables.TryGetValue(name, out variable))
                    return true;
            }
            variable = null;
            return false;
        }

        internal object GetVariableValue(string name)
        {
            var scope = this;
            while (scope != null)
            {
                if (scope.IsThreadSafeScope)
                {
                    if (scope.SafeVariables.TryGetValue(name, out var variable))
                        return variable.Value;
                }
                else
                {
                    if (scope.UnsafeVariables.TryGetValue(name, out var variable))
                        return variable.Value;
                }
                scope = scope.ParentScope;
            }

            if (Options.AllowUndefinedReferenceAccess)
                return Options.NoUndefined ? null : Undefined.Value;
            Exceptions.ThrowVariableIsNotDefined(name);
            return null;
        }

        internal object GetValue(object value)
        {
            if (value is TopazArrayWrapper arrayWrapper)
                value = arrayWrapper.UnwrapArray();
            else if (value is TopazObjectWrapper objectWrapper)
                value = objectWrapper.UnwrapObject();
            else if (value is TopazIdentifier identifier)
                return identifier.ScriptExecutor.GetVariableValue(identifier.Name);
            else if (value is TopazMemberAccessor memberAccessor)
                return memberAccessor.Execute(this);
            return value;
        }

        internal object GetVariableNameOrValue(object value , bool getVariableName)
        {
            if (getVariableName && value is TopazIdentifier identifier)
            {
                return identifier.Name;
            }
            return GetValue(value);
        }

        internal object GetMemberValue(object instance, object property, bool computed, bool optional)
        {
            var obj = GetValue(instance);
            var member = GetVariableNameOrValue(property, !computed);


            if (obj == null)
            {
                if (optional)
                    return null;
                if (Options.AllowNullReferenceMemberAccess)
                    return GetNullOrUndefined();
                Exceptions.ThrowCannotReadPropertiesOfNull(member);
            }

            if (!Options.SecurityPolicy.HasFlag(SecurityPolicy.EnableReflection) &&
                obj.GetType().Namespace.StartsWith("System.Reflection"))
                Exceptions.ThrowReflectionSecurityException(instance, property);

            if (obj is ITypeProxy typeProxy)
            {
                if (typeProxy
                    .TryGetStaticMember(
                    member?.ToString(),
                    out var staticMemberValue,
                    computed))
                    return staticMemberValue;
                return GetNullOrUndefined();
            }

            if (obj is Undefined)
            {
                if (Options.AllowUndefinedReferenceMemberAccess)
                    return GetNullOrUndefined();
                Exceptions.ThrowReferenceIsUndefined(instance);
            }

            if (TopazEngine.TryGetObjectMember(obj, member, out var value, computed))
                return value;

            return GetNullOrUndefined();
        }
    }
}

using Esprima.Ast;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Core
{
    internal partial class ScriptExecutor
    {
        internal void DefineVariable(
            object identifierOrReference,
            object value,
            VariableKind kind,
            VariableState state = VariableState.None)
        {
            if (identifierOrReference is Identifier identifier)
            {
                DefineVariable(identifier.Name, value, kind, state);
                return;
            }
            if (identifierOrReference is TopazIdentifier topazIdentifier)
            {
                DefineVariable(topazIdentifier.Name, value, kind, state);
                return;
            }
            if (identifierOrReference is string str)
            {
                DefineVariable(str, value, kind, state);
                return;
            }
            Exceptions.ThrowCannotDefineVariableWithGivenObject(identifierOrReference);
        }

        internal void DefineVariable(
            string name,
            object value,
            VariableKind kind,
            VariableState state = VariableState.None)
        {
            if (string.IsNullOrWhiteSpace(name))
                Exceptions.ThrowVariableNameCannotBeNullOrWhitespace();

            if (TryGetVariableInTheScope(name, out var variable) &&
                kind != VariableKind.Var &&
                variable.State != VariableState.Captured)
                Exceptions.ThrowVariableIsAlreadyDefined(name, this);
            AddOrUpdateVariableValueAndKindInTheScope(name, value, kind, state);
        }
    }
}

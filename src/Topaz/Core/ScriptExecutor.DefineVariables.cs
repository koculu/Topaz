using Esprima.Ast;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Core;

internal sealed partial class ScriptExecutor
{
    internal void DefineVariable(
        object identifierOrReference,
        object value,
        VariableKind kind,
        VariableState state = VariableState.None)
    {
        if (identifierOrReference is Identifier identifier)
        {
            DefineVariable(identifier, value, kind, state);
            return;
        }
        if (identifierOrReference is TopazIdentifier topazIdentifier)
        {
            DefineVariable(topazIdentifier, value, kind, state);
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

        if (!isEmptyScope && TryGetVariableInTheScope(name, out var variable) &&
            kind != VariableKind.Var &&
            variable.State != VariableState.Captured)
            Exceptions.ThrowVariableIsAlreadyDefined(name, this);
        AddOrUpdateVariableValueAndKindInTheScope(name, value, kind, state);
    }

    internal void DefineVariable(
        TopazIdentifier topazIdentifier,
        object value,
        VariableKind kind,
        VariableState state = VariableState.None)
    {
        if (topazIdentifier == null)
            return;
        var name = topazIdentifier.Name;
        DefineVariable(name, value, kind, state);
        topazIdentifier.InvalidateLocalCache();
    }

    internal void DefineVariable(
       Identifier identifier,
       object value,
       VariableKind kind,
       VariableState state = VariableState.None)
    {
        if (identifier == null)
            return;
        var name = identifier.Name;
        DefineVariable(name, value, kind, state);
        identifier.TopazIdentifier.InvalidateLocalCache();
    }
}

namespace Tenray.Topaz.Options;

public static class PresetOptions
{
    /// <summary>
    /// Default style
    /// </summary>
    public static TopazEngineOptions FriendlyStyle =>
           new()
           {
               AllowNullReferenceMemberAccess = true,
               AllowUndefinedReferenceAccess = true,
               AllowUndefinedReferenceMemberAccess = true,
               AssignmentWithoutDefinitionBehavior =
                   AssignmentWithoutDefinitionBehavior.DefineAsVarInExecutionScope,
               NoUndefined = true,
               VarScopeBehavior = VarScopeBehavior.FunctionScope
           };

    public static TopazEngineOptions EcmaJavascript =>
        new()
        {
            AllowNullReferenceMemberAccess = false,
            AllowUndefinedReferenceAccess = false,
            AllowUndefinedReferenceMemberAccess = false,
            AssignmentWithoutDefinitionBehavior =
                AssignmentWithoutDefinitionBehavior.DefineAsVarInGlobalScope,
            NoUndefined = false,
            VarScopeBehavior = VarScopeBehavior.FunctionScope
        };
    
    public static TopazEngineOptions EarlyErrorCatchStyle =>
        new()
        {
            AllowNullReferenceMemberAccess = false,
            AllowUndefinedReferenceAccess = true,
            AllowUndefinedReferenceMemberAccess = false,
            AssignmentWithoutDefinitionBehavior =
                AssignmentWithoutDefinitionBehavior.DefineAsVarInExecutionScope,
            NoUndefined = true,
            VarScopeBehavior = VarScopeBehavior.FunctionScope
        };
}

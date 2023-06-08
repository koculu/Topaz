namespace Tenray.Topaz.Options;

public enum AssignmentWithoutDefinitionBehavior
{
    /// <summary>
    /// Suggested and default parameter
    /// </summary>
    DefineAsVarInExecutionScope,

    /// <summary>
    /// ECMA behavior, not good for multithreaded execution environment
    /// </summary>
    DefineAsVarInGlobalScope,

    /// <summary>
    /// Slightly adjusted ECMA behavior to protect global scope
    /// from frequent changes. Variable definition occurs in
    /// the top level scope right before global scope.
    /// Since Function scope (Closure) is frozen,
    /// if the first child of Global Scope is Function Scope,
    /// then variable definition occurs in FunctionInnerBlock Scope.
    /// </summary>
    DefineAsVarInFirstChildOfGlobalScope,

    /// <summary>
    /// Automatically assigns a let variable in the execution scope.
    /// Please note that let variables are expensive, 
    /// because they are captured (cloned) in function closures.
    /// </summary>
    DefineAsLetInExecutionScope,

    /// <summary>
    /// Strict mode behavior which throws exception. 
    /// </summary>
    ThrowException
}

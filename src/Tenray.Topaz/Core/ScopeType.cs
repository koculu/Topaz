namespace Tenray.Topaz.Core;

internal enum ScopeType
{
    Global,
    Block,
    /// <summary>
    /// Function Scope (Closure).
    /// This scope is frozen.
    /// Its variable dictionary is immutable.
    /// </summary>
    Function,
    FunctionInnerBlock,
    Custom
}

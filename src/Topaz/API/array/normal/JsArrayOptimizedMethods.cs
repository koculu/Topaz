namespace Tenray.Topaz.API;

/// <summary>
/// The methods that are optimized by avoiding reflection on common array operations.
/// </summary>
internal enum JsArrayOptimizedMethods
{
    push,
    pop,
    shift
}

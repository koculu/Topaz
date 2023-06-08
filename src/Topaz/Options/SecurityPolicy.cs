using System;

namespace Tenray.Topaz.Options;

/// <summary>
/// Security Flags.
/// </summary>
[Flags]
public enum SecurityPolicy
{
    /// <summary>
    /// Default and secure. Script cannot process
    /// types in the System.Reflection namespace.
    /// </summary>
    Default,

    /// <summary>
    /// Reflection API is allowed.
    /// Script can access everything.
    /// Use it with caution.
    /// </summary>
    EnableReflection
}

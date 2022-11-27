namespace Tenray.Topaz.Interop;

/// <summary>
/// Provides member access security policy.
/// </summary>
public interface IMemberAccessPolicy
{
    /// <summary>
    /// Returns true if
    /// access allowed to
    /// given member name on given object instance.
    /// </summary>
    /// <param name="obj">Object instance.</param>
    /// <param name="memberName">Member name.</param>
    /// <returns>true if allowed, false otherwise.</returns>
    public bool IsObjectMemberAccessAllowed(object obj, string memberName);
}
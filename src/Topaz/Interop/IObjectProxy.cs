using System;

namespace Tenray.Topaz.Interop;

public interface IObjectProxy
{
    /// <summary>
    /// Returns true if a member is found, false otherwise.
    /// Output value is member of the given instance.
    /// If instance member is a function then output value is IInvokable.
    /// </summary>
    /// <param name="instance">Object instance.</param>
    /// <param name="member">Member name or indexed member value.</param>
    /// <param name="value">Returned value.</param>
    /// <param name="isIndexedProperty">Indicates if the member
    /// retrieval should be through an indexed property.</param>
    /// <returns></returns>
    bool TryGetObjectMember(
        object instance,
        object member,
        out object value,
        bool isIndexedProperty = false);

    /// <summary>
    /// Returns true if a member is found and updated with new value,
    /// false otherwise.
    /// If instance member is a function, returns false.
    /// </summary>
    /// <param name="instance">Object instance.</param>
    /// <param name="member">Member name or indexed member value.</param>
    /// <param name="value">New value.</param>
    /// <param name="isIndexedProperty">Indicates if the member
    /// retrieval should be through an indexed property.</param>
    /// <returns></returns>
    bool TrySetObjectMember(
        object instance,
        object member,
        object value,
        bool isIndexedProperty = false);
}

using System;
using System.Collections.Generic;

namespace Tenray.Topaz.Interop
{
    public interface ITypeProxy
    {
        /// <summary>
        /// Proxied type.
        /// </summary>
        Type ProxiedType { get; }

        /// <summary>
        ///  Calls generic or non-generic constructor.
        /// </summary>
        /// <param name="args">Generic and constructor arguments.</param>
        /// <returns></returns>
        object CallConstructor(IReadOnlyList<object> args);

        /// <summary>
        /// Returns true if a member is found, false otherwise.
        /// Output value is member of the static type.
        /// If static type member is a function then output value is IInvokable.
        /// </summary>
        /// <param name="member">Member name or indexed member value.</param>
        /// <param name="value">Returned value.</param>
        /// <param name="isIndexedProperty">Indicates if the member
        /// retrieval should be through an indexed property.</param>
        /// <returns></returns>
        bool TryGetStaticMember(
            object member,
            out object value,
            bool isIndexedProperty = false);

        /// <summary>
        /// Returns true if a member is found and updated with new value,
        /// false otherwise.
        /// If statyic type member is a function, returns false.
        /// </summary>
        /// <param name="member">Member name or indexed member value.</param>
        /// <param name="value">New value.</param>
        /// <param name="isIndexedProperty">Indicates if the member
        /// retrieval should be through an indexed property.</param>
        /// <returns></returns>
        bool TrySetStaticMember(
            object member,
            object value,
            bool isIndexedProperty = false);
    }
}

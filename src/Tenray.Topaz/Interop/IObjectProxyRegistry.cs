using System;

namespace Tenray.Topaz.Interop
{
    public interface IObjectProxyRegistry
    {
        void AddObjectProxy(Type type, IObjectProxy proxy);
        
        void RemoveObjectProxy(Type type);

        bool TryGetObjectProxy(object instance, out IObjectProxy proxy);
    }
}

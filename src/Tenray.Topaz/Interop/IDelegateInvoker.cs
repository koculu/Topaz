using System.Collections.Generic;

namespace Tenray.Topaz.Interop
{
    public interface IDelegateInvoker
    {
        object Invoke(object function, IReadOnlyList<object> args);
    }
}

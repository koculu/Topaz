using System.Collections.Generic;

namespace Tenray.Topaz.Interop;

public interface IInvokable
{
    object Invoke(IReadOnlyList<object> args);
}

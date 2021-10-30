using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tenray.Topaz.Interop
{
    public interface IInvokable
    {
        object Invoke(IReadOnlyList<object> args);
    }
    
}

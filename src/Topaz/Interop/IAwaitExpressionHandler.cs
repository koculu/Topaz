using System.Threading;
using System.Threading.Tasks;

namespace Tenray.Topaz.Interop;

public interface IAwaitExpressionHandler
{
    Task<object> HandleAwaitExpression(object awaitObject, CancellationToken token);
}
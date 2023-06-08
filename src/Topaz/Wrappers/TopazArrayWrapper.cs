using Tenray.Topaz.API;
using Tenray.Topaz.Core;

namespace Tenray.Topaz;

internal sealed class TopazArrayWrapper
{
    internal ScriptExecutor ScriptExecutor { get; }

    internal IJsArray WrappedArray { get; }

    bool isUnwrapped = false;

    internal TopazArrayWrapper(ScriptExecutor scriptExecutor, IJsArray array)
    {
        ScriptExecutor = scriptExecutor;
        WrappedArray = array;
    }

    internal object UnwrapArray()
    {
        var array = WrappedArray; 
        if (array == null)
            return null;
        if (isUnwrapped)
            return array;
        WrappedArray.UnwrapArray(ScriptExecutor);
        isUnwrapped = true;
        return array;
    }
}

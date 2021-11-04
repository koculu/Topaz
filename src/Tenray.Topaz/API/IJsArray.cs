using System.Collections;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.API
{
    public interface IJsArray
    {
        bool TryGetArrayValue(int index, out object value);

        void SetArrayValue(int index, object value);

        void AddArrayValue(object value);

        internal void UnwrapArray(ScriptExecutor scriptExecutor);
    }
}

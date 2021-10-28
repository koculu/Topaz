using System.Collections.Generic;
using System.Linq;
using Tenray.Topaz.Core;

namespace Tenray.Topaz
{
    internal class TopazObjectWrapper
    {
        internal ScriptExecutor ScriptExecutor { get; }

        internal IDictionary<string, object> WrappedObject { get; }

        bool isUnwrapped = false;

        internal TopazObjectWrapper(
            ScriptExecutor scriptExecutor,
            IDictionary<string, object> value)
        {
            ScriptExecutor = scriptExecutor;
            WrappedObject = value;
        }

        internal IDictionary<string, object> UnwrapObject()
        {
            var value = WrappedObject;
            if (value == null)
                return null;
            if (isUnwrapped)
                return value;
            var keys = value.Keys.ToArray();
            var len = keys.Length;
            for (var i = 0; i < len; ++i)
            {
                var key = keys[i];
                value[key] = ScriptExecutor.GetValue(value[key]);
            }
            isUnwrapped = true;
            return value;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Tenray.Topaz.API;
using Tenray.Topaz.Core;

namespace Tenray.Topaz
{
    internal sealed class TopazObjectWrapper
    {
        internal ScriptExecutor ScriptExecutor { get; }

        internal IJsObject WrappedObject { get; }

        bool isUnwrapped = false;

        internal TopazObjectWrapper(
            ScriptExecutor scriptExecutor,
            IJsObject value)
        {
            ScriptExecutor = scriptExecutor;
            WrappedObject = value;
        }

        internal IJsObject UnwrapObject()
        {
            var value = WrappedObject;
            if (value == null)
                return null;
            if (isUnwrapped)
                return value;
            value.UnwrapObject(ScriptExecutor);
            isUnwrapped = true;
            return value;
        }
    }
}

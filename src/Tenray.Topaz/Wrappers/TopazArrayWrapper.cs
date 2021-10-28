using System;
using System.Collections.Generic;
using Tenray.Topaz.Core;

namespace Tenray.Topaz
{
    internal class TopazArrayWrapper
    {
        internal ScriptExecutor ScriptExecutor { get; }

        internal List<object> WrappedArray { get; }

        bool isUnwrapped = false;

        internal TopazArrayWrapper(ScriptExecutor scriptExecutor, List<object> array)
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
            var len = array.Count;
            for (var i = 0; i < len; ++i)
            {
                array[i] = ScriptExecutor.GetValue(array[i]);
            }
            isUnwrapped = true;
            return array;
        }
    }
}

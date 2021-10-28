using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Wrappers
{
    internal class DynamicMemberFunctionWrapper
    {
        object Instance { get; }

        MethodInfo[] Methods { get; }

        internal DynamicMemberFunctionWrapper(object instance, MethodInfo[] methods)
        {
            Instance = instance;
            Methods = methods;
        }

        internal object InvokeFunction(IReadOnlyList<object> args)
        {
            if (Methods == null)
                return null;
            var method = FindBestMatch(args);
            if (method == null)
                return null;
            var parameters = method.GetParameters();
            var methodParamLength = parameters.Length;
            var argsArray = args as object[] ?? args.ToArray();

            argsArray = DynamicHelper.ArgumentArrayAdjustLength(argsArray, methodParamLength);
            for (var i = 0; i < methodParamLength; ++i)
            {
                var arg = argsArray[i];
                var p = parameters[i];
                if (arg is Undefined)
                {
                    argsArray[i] = null;
                    continue;
                }
                if (arg == null || arg.GetType() == p.ParameterType)
                    continue;
                argsArray[i] = Convert.ChangeType(arg, p.ParameterType);
            }
            return method.Invoke(Instance, argsArray);
        }

        private MethodInfo FindBestMatch(IReadOnlyList<object> args)
        {
            var methodCount = Methods.Length;
            if (methodCount == 1)
                return Methods[0];
            var argsLen = args.Count;
            for (var i = 0; i < methodCount; ++i)
            {
                var m = Methods[i];
                var p = m.GetParameters();
                if (p.Length == argsLen)
                    return m;
            }
            return null;
        }
    }
}

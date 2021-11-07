using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Interop
{
    public class InvokerUsingReflection : IInvokable
    {
        public readonly string Name;
        readonly object instance;
        readonly MethodInfo[] methodInfos;
        readonly ProxyOptions options;
        private readonly MethodAndParameterInfo extMethodParameterInfo;
        readonly ParameterInfo[][] allParameters;

        public InvokerUsingReflection(
            string name,
            MethodInfo[] methodInfos,
            object instance,
            ProxyOptions options,
            MethodAndParameterInfo extMethodParameterInfo = null)
        {
            Name = name;
            this.methodInfos = methodInfos;
            var len = methodInfos.Length;
            for (var i = 0; i < len; ++i)
            {
                var methodInfo = methodInfos[i];
                if (methodInfo.ContainsGenericParameters)
                {
                    var genericParamCount = 
                        methodInfo.GetGenericArguments().Length;
                    var genericArgs = Enumerable.Range(0, genericParamCount).Select(x => typeof(object)).ToArray();
                    methodInfos[i] = methodInfo.MakeGenericMethod(genericArgs);
                }
            }
            this.instance = instance;
            this.options = options;
            this.extMethodParameterInfo = extMethodParameterInfo;
            allParameters = methodInfos
                     .Select(x => x.GetParameters())
                     .ToArray();
        }

        public object Invoke(IReadOnlyList<object> args)
        {
            var convertStringsToEnum =
                options.HasFlag(ProxyOptions.ConvertStringArgumentsToEnum);
            if (!options.HasFlag(ProxyOptions.AutomaticTypeConversion))
            {
                if (ArgumentMatcher.TryFindBestMatch(
                   args,
                   allParameters,
                   out var index))
                {
                    var bestMethod = methodInfos[index];
                    return bestMethod.Invoke(instance, args as object[] ?? args.ToArray());
                }
                Exceptions
                    .ThrowCannotFindFunctionMatchingGivenArguments(
                    Name, args);
            }

            if (ArgumentMatcher.TryFindBestMatchWithTypeConversion(
                args,
                allParameters,
                convertStringsToEnum,
                out var index2,
                out var convertedArgs2))
            {
                var bestMethod = methodInfos[index2];
                return bestMethod.Invoke(instance, convertedArgs2);
            }

            if (instance != null && extMethodParameterInfo != null)
            {
                return TryInvokeExtensionMethod(args);
            }
            Exceptions
                .ThrowCannotFindFunctionMatchingGivenArguments(
                Name, args);
            return null;
        }

        private object TryInvokeExtensionMethod(IReadOnlyList<object> args)
        {
            var convertStringsToEnum =
                   options.HasFlag(ProxyOptions.ConvertStringArgumentsToEnum);
            var mpinfo = extMethodParameterInfo;
            if (!mpinfo.HasAny)
            {
                Exceptions
                    .ThrowCannotFindFunctionMatchingGivenArguments(
                    Name, args);
            }
            var extAllParameters = mpinfo.ParameterInfos;

            var orgArgs = args;
            var newArgs = new object[args.Count + 1];
            var len = args.Count;
            for (var i = 0; i < len; ++i)
                newArgs[i + 1] = args[i];
            newArgs[0] = instance;
            args = newArgs;
            if (!options.HasFlag(ProxyOptions.AutomaticTypeConversion))
            {
                if (ArgumentMatcher.TryFindBestMatch(
                   args,
                   extAllParameters,
                   out var index))
                {
                    var bestMethod = mpinfo.MethodInfos[index];
                    return bestMethod.Invoke(null, args as object[] ?? args.ToArray());
                }
                Exceptions
                    .ThrowCannotFindFunctionMatchingGivenArguments(
                    Name, orgArgs);
            }

            if (ArgumentMatcher.TryFindBestMatchWithTypeConversion(
                args,
                extAllParameters,
                convertStringsToEnum,
                out var index2,
                out var convertedArgs2))
            {
                var bestMethod = mpinfo.MethodInfos[index2];
                return bestMethod.Invoke(null, convertedArgs2);
            }

            Exceptions
                .ThrowCannotFindFunctionMatchingGivenArguments(
                Name, orgArgs);
            return null;
        }
    }
}

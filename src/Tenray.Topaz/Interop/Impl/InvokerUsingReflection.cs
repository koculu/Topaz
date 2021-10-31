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
        readonly ParameterInfo[][] allParameters;

        public InvokerUsingReflection(
            string name,
            MethodInfo[] methodInfos,
            object instance,
            ProxyOptions options)
        {
            Name = name;
            this.methodInfos = methodInfos;
            this.instance = instance;
            this.options = options; 
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
            Exceptions
                .ThrowCannotFindFunctionMatchingGivenArguments(
                Name, args);
            return null;
        }
    }
}

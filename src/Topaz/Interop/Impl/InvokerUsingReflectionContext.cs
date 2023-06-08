using System.Linq;
using System.Reflection;

namespace Tenray.Topaz.Interop;

public sealed class InvokerUsingReflectionContext
{
    public readonly string Name;

    public readonly MethodInfo[] MethodInfos;

    public readonly ProxyOptions Options;

    public readonly IValueConverter ValueConverter;

    public readonly MethodAndParameterInfo ExtMethodParameterInfo;

    public readonly ParameterInfo[][] AllParameters;

    public InvokerUsingReflectionContext(
        string name,
        MethodInfo[] methodInfos,
        ProxyOptions options,
        IValueConverter valueConverter,
        MethodAndParameterInfo extMethodParameterInfo = null)
    {
        Name = name;
        ValueConverter = valueConverter;
        MethodInfos = methodInfos;
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
        
        Options = options;
        ExtMethodParameterInfo = extMethodParameterInfo;
        AllParameters = methodInfos
                 .Select(x => x.GetParameters())
                 .ToArray();
    }

    public InvokerUsingReflection ToInvoker(object instance)
    {
        return new InvokerUsingReflection(this, instance);
    }
}

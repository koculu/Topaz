using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Tenray.Topaz.Interop;

public static class DynamicDelagateFactory
{
    static MemberInfo[] voidMethods = typeof(DynamicDelagateProxy)
            .GetMember("CallAction", BindingFlags.NonPublic | BindingFlags.Instance);

    static MemberInfo[] nonVoidMethods = typeof(DynamicDelagateProxy)
            .GetMember("CallFunc", BindingFlags.NonPublic | BindingFlags.Instance);

    public static object CreateDynamicDelegate(
        Type[] args,
        Type returnType,
        Func<object[], object> actualFunction,
        IValueConverter valueConverter
        )
    {
        var isVoid = returnType == typeof(void);
        if (!isVoid)
            args = args.Take(args.Length - 1).ToArray();
        var availableMethods = isVoid ? voidMethods : nonVoidMethods;
        var targetMethod = (MethodInfo)availableMethods
            .Where(x =>
                (x as MethodInfo)
                .GetParameters().Length == args.Length)
            .FirstOrDefault();
        if (targetMethod == null)
            throw new TopazException($"Can not create dynamic delegate. Argument length {args.Length} is not supported.");

        var parameters = new ParameterExpression[args.Length];
        var convertedParameters = new Expression[args.Length];
        for (var i = 0; i < args.Length; ++i)
        {
            var p = Expression.Parameter(args[i]);
            parameters[i] = p;
            convertedParameters[i] = Expression.Convert(p, typeof(object));
        }

        var proxy = new DynamicDelagateProxy(actualFunction, returnType, valueConverter);
        var instanceVar = Expression.Constant(proxy);
        var body = Expression
            .Call(instanceVar, targetMethod, convertedParameters);

        Expression convertedBody
            = returnType == targetMethod.ReturnType
            ? body
            : Expression.Convert(body, returnType);
        var expr = Expression
            .Lambda(convertedBody, parameters.ToArray());
        return expr.Compile();
    }
}

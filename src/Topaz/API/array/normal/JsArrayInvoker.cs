using System;
using System.Collections.Generic;
using System.Linq;
using Tenray.Topaz.Interop;

namespace Tenray.Topaz.API;

/// <summary>
/// Fast invoker for critical methods. The remaining methods would be invoked via default invoker.
/// </summary>
internal sealed class JsArrayInvoker : IInvokable
{
    readonly JsArray JsArray;

    readonly JsArrayOptimizedMethods Method;

    public JsArrayInvoker(JsArray instance, JsArrayOptimizedMethods method)
    {
        JsArray = instance;
        Method = method;
    }

    public object Invoke(IReadOnlyList<object> args)
    {
        var len = args.Count;
        switch (Method)
        {
            case JsArrayOptimizedMethods.push:
                if (len == 0)
                    return JsArray.Count;
                if (len == 1)
                    return JsArray.push(args[0]);
                return JsArray.push(args[0], args.ToArray()[1..]);
            case JsArrayOptimizedMethods.pop:
                return JsArray.pop();
            case JsArrayOptimizedMethods.shift:
                return JsArray.shift();
            default:
                throw new NotImplementedException($"JsArray.{Method} invoker is not implemented");
        }
    }
}

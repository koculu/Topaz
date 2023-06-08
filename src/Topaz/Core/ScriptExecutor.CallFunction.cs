using Esprima.Ast;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Interop;

namespace Tenray.Topaz.Core;

internal sealed partial class ScriptExecutor
{
    internal object CallFunction(object callee, IReadOnlyList<object> args, bool optional, CancellationToken token)
    {
        var value = GetValue(callee);
        if (value == null)
        {
            if (optional)
                return GetNullOrUndefined();
            Exceptions.ThrowFunctionIsNotDefined(callee, this);
        }

        if (value is TopazFunction topazFunction)
        {
            return topazFunction.Execute(args, token);
        }

        if (value is IInvokable invokable)
        {
            return invokable.Invoke(args);
        }

        return TopazEngine.DelegateInvoker.Invoke(value, args);
    }

    internal async ValueTask<object> CallFunctionAsync(object callee, IReadOnlyList<object> args, bool optional, CancellationToken token)
    {
        var value = GetValue(callee);
        if (value == null)
        {
            if (optional)
                return GetNullOrUndefined();
            Exceptions.ThrowFunctionIsNotDefined(callee, this);
        }

        if (value is TopazFunction topazFunction)
        {
            return await topazFunction.ExecuteAsync(args, token);
        }

        if (value is IInvokable invokable)
        {
            return invokable.Invoke(args);
        }

        return TopazEngine.DelegateInvoker.Invoke(value, args);
    }

    internal object GetNullOrUndefined()
    {
        return Options.NoUndefined ? null : Undefined.Value;
    }
}

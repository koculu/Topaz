using Esprima.Ast;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Interop;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Core
{
    internal partial class ScriptExecutor
    {
        private void CaptureVariables()
        {
            var scope = ParentScope;
            var capturedKeys = new HashSet<string>();
            while (scope != null)
            {
                KeyValuePair<string, Variable>[] list;
                if (scope.IsThreadSafeScope)
                    list = scope.SafeVariables.ToArray();
                else
                    list = scope.UnsafeVariables.ToArray();
                var len = list.Length;
                for (var i = 0; i < len; ++i)
                {
                    var variable = list[i].Value;
                    if (!variable.ShouldCapture)
                        continue;
                    var key = list[i].Key;
                    if (!capturedKeys.Contains(key))
                    {
                        AddOrUpdateVariableValueAndKindInTheScope(
                            key,
                            variable.Value,
                            variable.Kind,
                            VariableState.Captured);
                        capturedKeys.Add(key);
                    }
                }
                scope = scope.ParentScope;
            }
            IsFrozen = true;
        }

        internal object GetNullOrUndefined()
        {
            return Options.NoUndefined ? null : Undefined.Value;
        }

        internal object CallFunction(object callee, IReadOnlyList<object> args, bool optional)
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
                return topazFunction.ScriptExecutor.GetValue(topazFunction.Execute(args));
            }

            if (value is IInvokable invokable)
            {
                return invokable.Invoke(args.ToArray());
            }

            return DelegateInvoker.InvokeFunction(value, args);
        }

        internal async ValueTask<object> CallFunctionAsync(object callee, IReadOnlyList<object> args, bool optional)
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
                return topazFunction.ScriptExecutor.GetValue(
                    await topazFunction.ExecuteAsync(args));
            }

            if (value is IInvokable invokable)
            {
                return invokable.Invoke(args.ToArray());
            }

            return DelegateInvoker.InvokeFunction(value, args);
        }
    }
}

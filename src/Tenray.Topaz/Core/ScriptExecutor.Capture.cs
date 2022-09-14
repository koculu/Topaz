using Esprima.Ast;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Interop;

namespace Tenray.Topaz.Core
{
    internal sealed partial class ScriptExecutor
    {
        private void CaptureVariables()
        {
            // Closure and its ancesstors should not return to pool.
            MarkCanNotReturnToPool();
            var scope = ParentScope;
            var capturedKeys = new HashSet<string>();
            while (scope != null)
            {
                if (scope.isEmptyScope)
                {
                    scope = scope.ParentScope;
                    continue;
                }
                KeyValuePair<string, Variable>[] list;
                if (scope.IsThreadSafeScope)
                    list = scope.SafeVariables.ToArray();
                else
                    list = scope.UnsafeVariables.ToArray();
                var len = list.Length;
                for (var i = 0; i < len; ++i)
                {
                    var variable = list[i].Value;
                    var key = list[i].Key;
                    if (!variable.ShouldCapture)
                    {
                        capturedKeys.Add(key);
                        continue;
                    }
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
    }
}

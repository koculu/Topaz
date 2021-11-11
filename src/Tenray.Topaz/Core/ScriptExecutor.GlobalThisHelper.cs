using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tenray.Topaz.Core
{
    internal partial class ScriptExecutor
    {
        internal IDictionaryEnumerator GetScopeEnumerator()
        {
            var scope = this;
            // TODO: create variable value enumerator.
            if (scope.IsThreadSafeScope)
            {
                return ((IDictionary)(scope.SafeVariables)).GetEnumerator();
            }
            else
            {
                return scope.UnsafeVariables.GetEnumerator();
            }
        }

        internal bool RemoveVariableInTheScope(string name)
        {
            var scope = this;
            if (scope.IsThreadSafeScope)
            {
                return scope.SafeVariables.TryRemove(name, out _);
            }
            else
            {
                return scope.UnsafeVariables.Remove(name);
            }
        }

        internal void ClearScope()
        {
            var scope = this;
            if (scope.IsThreadSafeScope)
            {
                scope.SafeVariables.Clear();
            }
            else
            {
                scope.UnsafeVariables.Clear();
            }
        }

        internal void CopyScopeVariablesTo(Array array, int index)
        {
            var scope = this;
            if (scope.IsThreadSafeScope)
            {
                ((IDictionary)(scope.SafeVariables)).CopyTo(array, index);
            }
            else
            {
                throw new NotSupportedException("Unsafe Scope: CopyTo(Array,index) method is not supported.");
            }
        }

        internal int GetVariableCountInTheScope()
        {
            var scope = this;
            if (scope.IsThreadSafeScope)
            {
                if (scope.SafeVariables == null)
                    return 0;
                return scope.SafeVariables.Count;
            }
            else
            {
                if (scope.UnsafeVariables == null)
                    return 0;
                return scope.UnsafeVariables.Count;
            }
        }

        internal ICollection GetKeysInTheScope()
        {
            var scope = this;
            if (scope.IsThreadSafeScope)
            {
                return scope.SafeVariables.Keys.ToArray();
            }
            else
            {
                var dictionary = scope.UnsafeVariables;
                var list = new List<string>(dictionary.Count);
                foreach (var entry in dictionary)
                {
                    list.Add(entry.Key);
                }
                return list;
            }
        }

        internal ICollection GetValuesInTheScope()
        {
            var scope = this;
            if (scope.IsThreadSafeScope)
            {
                return scope.SafeVariables.Values.ToArray().Select(x => x.Value).ToArray();
            }
            else
            {
                var dictionary = scope.UnsafeVariables;
                var list = new List<object>(dictionary.Count);
                foreach (var entry in dictionary)
                {
                    list.Add(entry.Value.Value);
                }
                return list;
            }
        }
    }
}

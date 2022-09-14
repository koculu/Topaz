using System;
using System.Collections;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.API
{
    public sealed class GlobalThis : IDictionary
    {
        private const string NullString = "null";

        ScriptExecutor Scope { get; }

        public GlobalThis(ITopazEngineScope scope)
        {
            Scope = (ScriptExecutor)scope;
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => Scope.IsReadOnly;

        public ICollection Keys => Scope.GetKeysInTheScope();

        public ICollection Values => Scope.GetValuesInTheScope();

        public int Count => Scope.GetVariableCountInTheScope();

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        public object this[object key] {
            get
            {
                if (key == null)
                    key = NullString;
                if (Scope.TryGetVariableInTheScope(key.ToString(), out var value))
                    return value.Value;
                return Scope.GetNullOrUndefined();
            }
            set
            {
                if (key == null)
                    key = NullString;
                Scope.AddOrUpdateVariableValueInTheScope(key.ToString(), value, VariableKind.Var);
            }
        }
        
        public void Add(object key, object value)
        {
            this[key] = value;
        }

        public void Clear()
        {
            Scope.ClearScope();
        }

        public bool Contains(object key)
        {
            if (key == null)
                key = NullString;
            return Scope.TryGetVariableInTheScope(key.ToString(), out _);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return Scope.GetScopeEnumerator();
        }

        public void Remove(object key)
        {
            if (key == null)
                key = NullString;
            Scope.RemoveVariableInTheScope(key.ToString());
        }

        public void CopyTo(Array array, int index)
        {
            Scope.CopyScopeVariablesTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Scope.GetScopeEnumerator();
        }
    }
}

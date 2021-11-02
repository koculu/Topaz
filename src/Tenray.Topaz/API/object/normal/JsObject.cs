using Microsoft.Collections.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.API
{
    public partial class JsObject : IJsObject, IDictionary
    {
        private const string NullString = "null";

        readonly DictionarySlim<string, object> dictionary = new();

        readonly bool noUndefined;

        public JsObject(TopazEngine engine)
        {
            noUndefined = engine.Options.NoUndefined;
        }

        public object this[object key] { 
            get
            {
                if (key == null)
                    key = NullString;
                if (dictionary.TryGetValue(key.ToString(), out var value))
                    return value;
                return NullOrUndefined;
            }
            set 
            {
                Add(key, ConvertJsonElementToNetObject(value));
            }
        }

        private Undefined NullOrUndefined => noUndefined ? null : Undefined.Value;

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public ICollection Keys => ((IDictionary)dictionary).Keys;

        public ICollection Values => ((IDictionary)dictionary).Keys;

        public int Count => dictionary.Count;

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        public void Add(object key, object value)
        {
            if (key == null)
                key = NullString;
            ref var entry = ref dictionary.GetOrAddValueRef(key.ToString());
            entry = value;
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(object key)
        {
            return dictionary.ContainsKey(key.ToString());
        }

        public void CopyTo(Array array, int index)
        {
            ((IDictionary)dictionary).CopyTo(array, index);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return ((IDictionary)dictionary).GetEnumerator();
        }

        public void Remove(object key)
        {
            if (key == null)
                key = NullString;
            ((IDictionary)dictionary).Remove(key.ToString());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary)dictionary).GetEnumerator();
        }

        public bool TryGetValue(object key, out object value)
        {
            if (key == null)
                key = NullString;
            if (dictionary.TryGetValue(key.ToString(), out value))
                return true;
            value = NullOrUndefined;
            return false;
        }

        public IEnumerable GetObjectKeys()
        {
            return Keys;
        }

        void IJsObject.UnwrapObject(ScriptExecutor scriptExecutor)
        {
            foreach (var key in Keys)
            {
                var skey = (string)key;
                ref var entry = ref dictionary.GetOrAddValueRef(skey);
                entry = scriptExecutor.GetValue(entry);
            }
        }

        public void SetValue(object key, object value)
        {
            if (key == null)
                key = NullString;
            var skey = (string)key;
            ref var entry = ref dictionary.GetOrAddValueRef(skey);
            entry = value;
        }
    }
}

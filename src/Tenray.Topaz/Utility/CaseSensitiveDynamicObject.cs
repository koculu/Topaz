using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Tenray.Topaz.Utility
{
    public class CaseSensitiveDynamicObject : DynamicObject, IDictionary<string, object>, IDictionary
    {
        public Dictionary<string, object> Dictionary
            = new();

        public ICollection<string> Keys => Dictionary.Keys;

        public ICollection<object> Values => Dictionary.Values;

        public int Count => Dictionary.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        ICollection IDictionary.Keys => (ICollection)Keys;

        ICollection IDictionary.Values => (ICollection)Values;

        public bool IsSynchronized => false;

        public object SyncRoot => new();

        public object this[object key]
        {
            get => this[key.ToString()];
            set => this[key.ToString()] = value;
        }

        public object this[string key]
        {
            get => Dictionary[key];
            set => Dictionary[key] = ConvertJsonElementToNetObject(value);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            value = ConvertJsonElementToNetObject(value);
            Dictionary[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Dictionary.TryGetValue(binder.Name, out result))
            {
                return true;
            }
            result = null;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Dictionary.Keys.ToArray();
        }

        public void Add(string key, object value)
        {
            Dictionary.Add(key, ConvertJsonElementToNetObject(value));
        }

        public bool ContainsKey(string key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Dictionary.Remove(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Dictionary.Add(item.Key, ConvertJsonElementToNetObject(item.Value));
        }

        public void Clear()
        {
            Dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary)Dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Dictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public static CaseSensitiveDynamicObject ToLowerCaseObject(object data)
        {
            if (data == null)
                return null;
            var result = new CaseSensitiveDynamicObject();
            var type = data.GetType();
            var props = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.GetProperty);
            foreach (var prop in props)
            {
                var value = prop.GetValue(data);
                var lowerCase = char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..];
                result.Dictionary[lowerCase] = value;
            }
            return result;
        }

        public void Merge(CaseSensitiveDynamicObject model, bool overwrite = false)
        {
            if (model == null)
                return;
            var dic = Dictionary;
            foreach (var (key, value) in model.Dictionary)
            {
                if (!overwrite && dic.ContainsKey(key))
                    continue;
                dic[key] = value;
            }
        }

        private static object ConvertJsonElementToNetObject(object value)
        {
            if (value == null)
                return null;
            if (value is JsonElement jsonElement)
                return jsonElement.ConvertToNetObject();
            return value;
        }

        public void Add(object key, object value)
        {
            Add(key.ToString(), value);
        }

        public bool Contains(object key)
        {
            return ContainsKey(key.ToString());
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public void Remove(object key)
        {
            Remove(key.ToString());
        }

        public void CopyTo(Array array, int index)
        {
            ((IDictionary)Dictionary).CopyTo(array, index);
        }
    }

}

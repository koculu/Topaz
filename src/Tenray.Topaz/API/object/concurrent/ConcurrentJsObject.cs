using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.API;

public partial class ConcurrentJsObject : IJsObject, IDictionary
{
    private const string NullString = "null";

    // ConcurrentDictionary enumeration key order is NOT guaranteed.
    // Hence, serialization and deserialization becomes flaky. Find out a solution!
    readonly ConcurrentDictionary<string, object> dictionary = new();

    public object this[object key] { 
        get
        {
            if (key == null)
                key = NullString;
            if (dictionary.TryGetValue(key.ToString(), out var value))
                return value;
            return Undefined.Value;
        }
        set 
        {
            Add(key, ConvertJsonElementToConcurrentJsObject(value));
        }
    }

    public bool IsFixedSize => false;

    public bool IsReadOnly => false;

    public ICollection Keys => ((IDictionary)dictionary).Keys;

    public ICollection Values => ((IDictionary)dictionary).Values;

    public int Count => dictionary.Count;

    public bool IsSynchronized => false;

    public object SyncRoot => null;

    public void Add(object key, object value)
    {
        if (key == null)
            key = NullString;
        dictionary.AddOrUpdate(key.ToString(), value,
            (key, oldValue) =>
            {
                return value;
            });
    }

    public void Clear()
    {
        dictionary.Clear();
    }

    public bool Contains(object key)
    {
        if (key == null)
            key = NullString;
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
        return dictionary.TryGetValue(key.ToString(), out value);
    }

    public IEnumerable GetObjectKeys()
    {
        return GetObjectKeysInternal();
    }

    protected virtual IEnumerable GetObjectKeysInternal()
    {
        return Keys;
    }

    void IJsObject.UnwrapObject(ScriptExecutor scriptExecutor)
    {
        var keys = dictionary.Keys.ToArray();
        foreach (var key in keys)
        {
            var value = scriptExecutor.GetValue(dictionary[key]);
            Add(key, value);
        }
    }

    public void SetValue(object key, object value)
    {
        if (key == null)
            key = NullString;
        dictionary.AddOrUpdate(key.ToString(), value,
            (key, oldValue) =>
            {
                return value;
            });
    }

    bool IJsObject.IsPrototypeProperty(object member)
    {
        if (member == null)
            return false;
        if (member is not string memberName)
            return false;
        return IsPrototypePropertyInternal(memberName);
    }

    protected virtual bool IsPrototypePropertyInternal(string memberName)
    {
        return false;
    }
}

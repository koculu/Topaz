using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.API;

public sealed partial class JsArray : JsObject, IJsArray, IList<object>
{
    readonly List<object> arraylist;

    public JsArray()
    {
        arraylist = new();
    }

    public JsArray(int capacity)
    {
        arraylist = new(capacity);
    }

    public JsArray(IReadOnlyList<object> items)
    {
        var len = items.Count;
        var list = arraylist = new(len);
        for (var i = 0; i < len; ++i)
        {
            list.Add(items[i]);
        }
    }

    public void AddArrayValues(IEnumerable enumerable)
    {
        foreach (var item in enumerable)
            AddArrayValue(item);
    }

    public void AddArrayValue(object value)
    {
        arraylist.Add(value);
    }

    public void SetArrayValue(int index, object value)
    {
        var list = arraylist;
        SetMinimumArraySize(arraylist, index + 1);
        list[index] = value;
    }

    public bool TryGetArrayValue(int index, out object value)
    {
        var list = arraylist;
        if (index < 0 || index >= list.Count)
        {
            value = Undefined.Value;
            return false;
        }
        value = list[index];
        return true;
    }

    void IJsArray.UnwrapArray(ScriptExecutor scriptExecutor)
    {
        var list = arraylist;
        var len = list.Count;
        for (var i = 0; i < len; ++i)
        {
            list[i] = scriptExecutor.GetValue(list[i]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return arraylist.GetEnumerator();
    }

    public object this[int index]
    {
        get
        {
            var list = arraylist;
            if (index < 0 || index >= list.Count)
            {
                return Undefined.Value;
            }
            return list[index];
        }
        set
        {
            SetMinimumArraySize(arraylist, index + 1);
            arraylist[index] = value;
        }
    }

    private static void SetMinimumArraySize(List<object> list, int minSize)
    {
        while (list.Count < minSize)
        {
            list.Add(null);
        }
    }

    private static void SetMaximumArraySize(List<object> list, int maxSize)
    {
        while (list.Count > maxSize)
        {
            list.RemoveAt(list.Count - 1);
        }
    }

    protected override IEnumerable GetObjectKeysInternal()
    {
        return Enumerable.Range(0, arraylist.Count);
    }

    int ICollection<object>.Count => arraylist.Count;

    bool ICollection<object>.IsReadOnly => false;

    void ICollection<object>.Add(object item)
    {
        arraylist.Add(ConvertJsonElementToJsObject(item));
    }

    void ICollection<object>.CopyTo(object[] array, int arrayIndex)
    {
        arraylist.CopyTo(array, arrayIndex);
    }

    bool ICollection<object>.Remove(object item)
    {
        return arraylist.Remove(item);
    }

    void ICollection<object>.Clear()
    {
        arraylist.Clear();
    }

    bool ICollection<object>.Contains(object item)
    {
        return arraylist.Contains(item);
    }

    IEnumerator<object> IEnumerable<object>.GetEnumerator()
    {
        return arraylist.GetEnumerator();
    }

    public int IndexOf(object item)
    {
        return arraylist.IndexOf(item);
    }

    public void Insert(int index, object item)
    {
        arraylist.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        arraylist.RemoveAt(index);
    }

    protected override bool IsPrototypePropertyInternal(string memberName)
    {
        return memberName switch
        {
            "length" => true,
            _ => false
        };
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.API
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "This is a JS prototype.")]
    public partial class JsArray
    {   
        public object at(int index)
        {
            if (index < 0)
                index = Count + index;
            return this[index];
        }

        public IJsArray concat(params IEnumerable[] arrays) {
            var result = new JsArray();
            result.AddArrayValues(arraylist);
            foreach (var arr in arrays)
            {
                result.AddArrayValues(arr);
            }
            return result;
        }

        public IJsArray copyWithin(int target)
        {
            return copyWithin(target, 0, arraylist.Count);
        }

        public IJsArray copyWithin(int target, int start)
        {
            return copyWithin(target, start, arraylist.Count);
        }

        public IJsArray copyWithin(int target, int start, int end) 
        {
            var list = arraylist;
            var len = list.Count;
            if (target < 0)
                target = len + target;
            if (start < 0)
                start = len + start;
            
            if (target == start)
                return this;

            if (end < 0)
                end = len + end; //exclusive end
            
            end = Math.Min(len, end);
            
            if (target < start)
            {
                var j = 0;
                for (var i = start; i < end; ++i)
                {
                    var k = target + j;
                    if (k >= len)
                        break;
                    list[k] = list[i];
                    ++j;
                }
            }
            else
            {
                var j = end - start - 1;
                for (var i = end - 1; i >= start; --i)
                {
                    var k = target + j;
                    if (k < len)
                        list[k] = list[i];
                    --j;
                }
            }
            
            return this;
        }

        public IEnumerable entries()
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var row = new JsArray();
                row.AddArrayValue(i);
                row.AddArrayValue(list[i]);
                yield return row;
            }
        }

        public bool every(Func<object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                if (JavascriptTypeUtility.IsObjectFalse(callbackFn(list[i])))
                    return false;
            }
            return true;
        }

        public bool every(Func<object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                if (JavascriptTypeUtility.IsObjectFalse(callbackFn(list[i], i)))
                    return false;
            }
            return true;
        }

        public bool every(Func<object, object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                if (JavascriptTypeUtility.IsObjectFalse(callbackFn(list[i], i, this)))
                    return false;
            }
            return true;
        }

        public IJsArray fill(object value) {
            var list = arraylist;
            var len = list.Count;
            for (var i = 0; i < len; ++i)
                list[i] = value;
            return this;
        }

        public IJsArray fill(object value, int start)
        {
            var list = arraylist;
            var len = list.Count;
            for (var i = start; i < len; ++i)
                list[i] = value;
            return this;
        }

        public IJsArray fill(object value, int start, int end)
        {
            // end is exclusive!
            var list = arraylist;
            var len = list.Count;
            if (len < end)
                end = len;
            for (var i = start; i < end; ++i)
                list[i] = value;
            return this;
        }

        public IJsArray filter(Func<object, object> callbackFn)
        {
            var result = new JsArray();
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item)))
                    result.AddArrayValue(item);
            }
            return result;
        }

        public IJsArray filter(Func<object, object, object> callbackFn)
        {
            var result = new JsArray();
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item, i)))
                    result.AddArrayValue(item);
            }
            return result;
        }

        public IJsArray filter(Func<object, object, object, object> callbackFn)
        {
            var result = new JsArray();
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item, i, this)))
                    result.AddArrayValue(item);
            }
            return result;
        }

        public object find(Func<object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item)))
                    return item;
            }
            return Undefined.Value;
        }

        public object find(Func<object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item, i)))
                    return item;
            }
            return Undefined.Value;
        }

        public object find(Func<object, object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item, i, this)))
                    return item;
            }
            return Undefined.Value;
        }

        public int findIndex(Func<object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item)))
                    return i;
            }
            return -1;
        }

        public int findIndex(Func<object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item, i)))
                    return i;
            }
            return -1;
        }

        public int findIndex(Func<object, object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item, i, this)))
                    return i;
            }
            return -1;
        }

        public IJsArray flat() {
            return flat(1);
        }

        private void processFlat(JsArray dest, JsArray array, int depth)
        {
            var list = array.arraylist;
            var len = list.Count;
            for (var i = 0; i < len; ++i)
            {
                var item = list[i];
                if (depth > 0 && item is JsArray inner)
                {
                    processFlat(dest, inner, depth - 1);
                }
                else
                {
                    dest.AddArrayValue(item);
                }
            }
        }

        public IJsArray flat(int depth) {
            var result = new JsArray();
            processFlat(result, this, depth);
            return result;
        }

        public IJsArray flatMap(Func<object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            var result = new JsArray();
            for (var i = 0; i < len; ++i)
            {
                var item = callbackFn(list[i]);
                if (item is JsArray)
                {
                    // jsArray is List and Dictionary at the same time.
                    // Hence, make sure to iterate through enumerable.
                    foreach (var innerItem in (IEnumerable)item)
                    {
                        result.AddArrayValue(innerItem);
                    }
                }
                else
                {
                    result.AddArrayValue(item);
                }
            }
            return result;
        }

        public IJsArray flatMap(Func<object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            var result = new JsArray();
            for (var i = 0; i < len; ++i)
            {
                var item = callbackFn(list[i], i);
                if (item is JsArray)
                {
                    // jsArray is List and Dictionary at the same time.
                    // Hence, make sure to iterate through enumerable.
                    foreach (var innerItem in (IEnumerable)item)
                    {
                        result.AddArrayValue(innerItem);
                    }
                }
                else
                {
                    result.AddArrayValue(item);
                }
            }
            return result;
        }

        public IJsArray flatMap(Func<object, object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            var result = new JsArray();
            for (var i = 0; i < len; ++i)
            {
                var item = callbackFn(list[i], i, this);
                if (item is JsArray)
                {
                    // jsArray is List and Dictionary at the same time.
                    // Hence, make sure to iterate through enumerable.
                    foreach (var innerItem in (IEnumerable)item)
                    {
                        result.AddArrayValue(innerItem);
                    }
                }
                else
                {
                    result.AddArrayValue(item);
                }
            }
            return result;
        }

        public void forEach(Action<object> callbackFn)
        {
            var list = arraylist;
            var len = list.Count;
            for(var i = 0; i < len; ++i)
            {
                callbackFn(list[i]);
            }
        }

        public void forEach(Action<object, object> callbackFn)
        {
            var list = arraylist;
            var len = list.Count;
            for (var i = 0; i < len; ++i)
            {
                callbackFn(list[i], i);
            }
        }

        public void forEach(Action<object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = list.Count;
            for (var i = 0; i < len; ++i)
            {
                callbackFn(list[i], i, this);
            }
        }

        public bool includes(object item)
        {
            return indexOf(item) != -1;
        }

        public bool includes(object item, int fromIndex)
        {
            return indexOf(item, fromIndex) != -1;
        }

        public int indexOf(object item) {
            return arraylist.IndexOf(item);
        }

        public int indexOf(object item, int fromIndex)
        {
            if (fromIndex < 0)
                fromIndex = arraylist.Count + fromIndex;
            return arraylist.IndexOf(item, fromIndex);
        }

        public string join() { 
            return string.Join(",", arraylist.ToArray()); 
        }

        public string join(string seperator) {
            return string.Join(seperator ?? ",", arraylist.ToArray());
        }

        public IEnumerable keys() {
            return Enumerable.Range(0, arraylist.Count);
        }

        public int lastIndexOf(object item)
        {
            return arraylist.LastIndexOf(item);
        }

        public int lastIndexOf(object item, int fromIndex)
        {
            if (fromIndex < 0)
                fromIndex = arraylist.Count + fromIndex;
            return arraylist.LastIndexOf(item, fromIndex);
        }

        public int length
        {
            get
            {
                return arraylist.Count;
            }
            set
            {
                SetMinimumArraySize(arraylist, value);
                SetMaximumArraySize(arraylist, value);
            }
        }

        public IJsArray map(Func<object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            var result = new JsArray();
            for (var i = 0; i < len; ++i)
            {
                var item = callbackFn(list[i]);
                result[i] = item;
            }
            return result;
        }

        public IJsArray map(Func<object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            var result = new JsArray();
            for (var i = 0; i < len; ++i)
            {
                var item = callbackFn(list[i], i);
                result[i] = item;
            }
            return result;
        }

        public IJsArray map(Func<object, object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            var result = new JsArray();
            for (var i = 0; i < len; ++i)
            {
                var item = callbackFn(list[i], i, this);
                result[i] = item;
            }
            return result;
        }

        public object pop() {
            var last = arraylist.Count - 1;
            if (last < 0)
                return Undefined.Value;
            var item = arraylist[last];
            arraylist.RemoveAt(last);
            return item;
        }

        public int push(object item, params object[] items) {
            AddArrayValue(item);
            AddArrayValues(items);
            return arraylist.Count;
        }

        public object reduce(Func<object, object, object> callbackFn)
        {
            return reduce(callbackFn, Undefined.Value);
        }

        public object reduce(Func<object, object, object> callbackFn, object initialValue)
        {
            var list = arraylist;
            var len = arraylist.Count;
            object previousValue = initialValue;
            var start = 0;
            if (initialValue == Undefined.Value)
            {
                if (len == 0)
                    Exceptions.ThrowReduceOfEmptyArrayWithNoInitialValue();
                start = 1;
                previousValue = list[0];
            }
            for (var i = start; i < len; ++i)
            {
                previousValue = callbackFn(previousValue, list[i]);
            }
            return previousValue;
        }

        public object reduce(Func<object, object, object, object> callbackFn)
        {
            return reduce(callbackFn, Undefined.Value);
        }

        public object reduce(Func<object, object, object, object> callbackFn, object initialValue)
        {
            var list = arraylist;
            var len = arraylist.Count;
            object previousValue = initialValue;
            var start = 0;
            if (initialValue == Undefined.Value)
            {
                if (len == 0)
                    Exceptions.ThrowReduceOfEmptyArrayWithNoInitialValue();
                start = 1;
                previousValue = list[0];
            }
            for (var i = start; i < len; ++i)
            {
                previousValue = callbackFn(
                    previousValue,
                    list[i],
                    i);
            }
            return previousValue;
        }

        public object reduce(Func<object, object, object, object, object> callbackFn)
        {
            return reduce(callbackFn, Undefined.Value);
        }

        public object reduce(Func<object, object, object, object, object> callbackFn, object initialValue)
        {
            var list = arraylist;
            var len = arraylist.Count;
            object previousValue = initialValue;
            var start = 0;
            if (initialValue == Undefined.Value)
            {
                if (len == 0)
                    Exceptions.ThrowReduceOfEmptyArrayWithNoInitialValue();
                start = 1;
                previousValue = list[0];
            }
            for (var i = start; i < len; ++i)
            {
                previousValue = callbackFn(
                    previousValue,
                    list[i],
                    i,
                    this);
            }
            return previousValue;
        }

        public object reduceRight(Func<object, object, object> callbackFn)
        {
            return reduceRight(callbackFn, Undefined.Value);
        }

        public object reduceRight(Func<object, object, object> callbackFn, object initialValue)
        {
            var list = arraylist;
            var len = arraylist.Count;
            object previousValue = initialValue;
            var start = len - 1;
            if (initialValue == Undefined.Value)
            {
                if (len == 0)
                    Exceptions.ThrowReduceOfEmptyArrayWithNoInitialValue();
                previousValue = list[start];
                --start;
            }
            for (var i = start; i >= 0; --i)
            {
                previousValue = callbackFn(previousValue, list[i]);
            }
            return previousValue;
        }

        public object reduceRight(Func<object, object, object, object> callbackFn)
        {
            return reduceRight(callbackFn, Undefined.Value);
        }

        public object reduceRight(Func<object, object, object, object> callbackFn, object initialValue)
        {
            var list = arraylist;
            var len = arraylist.Count;
            object previousValue = initialValue;
            var start = len - 1;
            if (initialValue == Undefined.Value)
            {
                if (len == 0)
                    Exceptions.ThrowReduceOfEmptyArrayWithNoInitialValue();
                previousValue = list[start];
                --start;
            }
            for (var i = start; i >= 0; --i)
            {
                previousValue = callbackFn(
                    previousValue,
                    list[i],
                    i);
            }
            return previousValue;
        }

        public object reduceRight(Func<object, object, object, object, object> callbackFn)
        {
            return reduceRight(callbackFn, Undefined.Value);
        }

        public object reduceRight(Func<object, object, object, object, object> callbackFn, object initialValue)
        {
            var list = arraylist;
            var len = arraylist.Count;
            object previousValue = initialValue;
            var start = len - 1;
            if (initialValue == Undefined.Value)
            {
                if (len == 0)
                    Exceptions.ThrowReduceOfEmptyArrayWithNoInitialValue();
                previousValue = list[start];
                --start;
            }
            for (var i = start; i >= 0; --i)
            {
                previousValue = callbackFn(
                    previousValue,
                    list[i],
                    i,
                    this);
            }
            return previousValue;
        }

        public IJsArray reverse() {
            var len = arraylist.Count;
            var mid = len / 2;
            for (var i = 0; i < mid; ++i)
            {
                var j = len - i - 1;
                var a = arraylist[i];
                arraylist[i] = arraylist[j];
                arraylist[j] = a;
            }
            return this;
        }

        public object shift()
        {
            if (arraylist.Count < 1)
                return Undefined.Value;
            var item = arraylist[0];
            arraylist.RemoveAt(0);
            return item;
        }

        public IJsArray slice() {
            return slice(0, arraylist.Count);
        }
        
        public IJsArray slice(int start)
        {
            return slice(start, arraylist.Count);
        }
        
        public IJsArray slice(int start, int end)
        {
            var list = arraylist;
            var len = list.Count;
            var result = new JsArray();
            if (start < 0)
                start = len + start;
            if (end < 0)
                end = len + end;
            end = Math.Min(len, end);
            for (var i = start; i < end; ++i)
            {
                result.AddArrayValue(list[i]);
            }
            return result;
        }

        public bool some(Func<object, object> callbackFn) {
            var list = arraylist;
            var len = list.Count;
            for (var i = 0; i < len; ++i)
            {
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(list[i])))
                    return true;
            }
            return false;
        }

        public bool some(Func<object, object, object> callbackFn)
        { 
            var list = arraylist;
            var len = list.Count;
            for (var i = 0; i < len; ++i)
            {
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(list[i], i)))
                    return true;
            }
            return false;
        }

        public bool some(Func<object, object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = list.Count;
            for (var i = 0; i < len; ++i)
            {
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(list[i], i, this)))
                    return true;
            }
            return false;
        }

        public IJsArray sort() {
            arraylist.Sort();
            return this;
        }

        public IJsArray sort(Func<object, object, object> compareFn)
        {
            var wrapper = new Func<object, object, int>((x, y) =>
            {
                return (int)Convert.ChangeType(compareFn(x, y), typeof(int));
            });
            arraylist.Sort(new Comparison<object>(wrapper));
            return this;
        }

        public void splice() { }

        public string toLocaleString() {
            return toString();
        }

        public override string toString() {
            var sb = new StringBuilder();
            var passedFirst = false;
            foreach (var x in arraylist)
            {
                if (passedFirst)
                    sb.Append(',');
                else 
                    passedFirst = true;
                if (x is JsObject jsObject)
                    sb.Append(jsObject.toString());
                else
                    sb.Append(x.ToString());
            }
            return sb.ToString();
        }

        public int unshift(object item, params object[] items)
        {
            var mergedItems = new object[items.Length + 1];
            mergedItems[0] = item;
            Array.Copy(items, 0, mergedItems, 1, items.Length);
            arraylist.InsertRange(0, mergedItems);
            return arraylist.Count;
        }

        public IEnumerable values() {
            foreach (var item in arraylist)
                yield return item;
        }
    }
}

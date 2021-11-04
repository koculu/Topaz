﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenray.Topaz.Core;

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

        public void constructor() { }

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
                end = len + end + 1; //exclusive end
            
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

        public IEnumerable entries() {
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                var row = new JsArray();
                row.AddArrayValue(i);
                row.AddArrayValue(arraylist[i]);
                yield return row;
            }
        }

        public bool every(Func<object, object> callbackFn)
        {
            foreach (var item in arraylist)
            {
                if (JavascriptTypeUtility.IsObjectFalse(callbackFn(item)))
                    return false;
            }
            return true;
        }

        public bool every(Func<object, object, object> callbackFn)
        {
            var i = 0;
            foreach (var item in arraylist)
            {
                if (JavascriptTypeUtility.IsObjectFalse(callbackFn(item, i++)))
                    return false;
            }
            return true;
        }

        public bool every(Func<object, object, object, object> callbackFn)
        {
            var i = 0;
            foreach (var item in arraylist)
            {
                if (JavascriptTypeUtility.IsObjectFalse(callbackFn(item, i++, this)))
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

        public void filter() { }
        public void find() { }
        public void findIndex() { }
        public void flat() { }
        public void flatMap() { }

        public void forEach(Action<object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for(var i = 0; i < len; ++i)
            {
                callbackFn(list[i]);
            }
        }

        public void forEach(Action<object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
            for (var i = 0; i < len; ++i)
            {
                callbackFn(list[i], i);
            }
        }

        public void forEach(Action<object, object, object> callbackFn)
        {
            var list = arraylist;
            var len = arraylist.Count;
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
            result.length = len;
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
            result.length = len;
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
            result.length = len;
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

        public void reduce() { }
        public void reduceRight() { }
        
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
        public void slice() { }

        public bool some(Func<object, object> callbackFn) {
            foreach (var item in arraylist)
            {
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item)))
                    return true;
            }
            return false;
        }

        public bool some(Func<object, object, object> callbackFn)
        { 
            var i = 0;
            foreach (var item in arraylist)
            {
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item, i++)))
                    return true;
            }
            return false;
        }

        public bool some(Func<object, object, object, object> callbackFn)
        {
            var i = 0;
            foreach (var item in arraylist)
            {
                if (JavascriptTypeUtility.IsObjectTrue(callbackFn(item, i++, this)))
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

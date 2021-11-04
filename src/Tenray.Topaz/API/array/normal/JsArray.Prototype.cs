using System;
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
        public void copyWithin() { }
        public void entries() { }
        public void every() { }
        public void fill() { }
        public void filter() { }
        public void find() { }
        public void findIndex() { }
        public void flat() { }
        public void flatMap() { }
        public void forEach() { }

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

        public void map() { }

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

        public bool some(Func<object, bool> callbackFn) {
            foreach (var item in arraylist)
            {
                if (callbackFn(item))
                    return true;
            }
            return false;
        }

        public bool some(Func<object, object, bool> callbackFn)
        { 
            var i = 0;
            foreach (var item in arraylist)
            {
                if (callbackFn(item, i++))
                    return true;
            }
            return false;
        }

        public bool some(Func<object, object, object, bool> callbackFn)
        {
            var i = 0;
            foreach (var item in arraylist)
            {
                if (callbackFn(item, i++, this))
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

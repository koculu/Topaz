using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public void join() { }
        public void keys() { }

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
        public void some() { }
        public void sort() { }
        public void splice() { }
        public void toLocaleString() { }
        public void toString() { }
        public void unshift() { }
        public void values() { }
    }
}

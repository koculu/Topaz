using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Tenray.Topaz.API;

namespace Tenray.Topaz.Interop
{
    public static class DynamicObjectKeysGetter
    {
        public static IEnumerable GetObjectKeys(object obj)
        {
            if (obj == null)
                return Array.Empty<object>();

            if (obj is IJsObject jsObject)
            {
                return jsObject.GetObjectKeys();
            }

            if (obj is IList list)
                return Enumerable.Range(0, list.Count);

            if (obj is IDictionary dic)
                return dic.Keys;

            return obj.GetType()
                .GetMembers(
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.GetField |
                    BindingFlags.GetProperty)
                .Where(x => !(
                    x is PropertyInfo pi &&
                    (pi.GetMethod == null || pi.GetMethod.IsPrivate)
                    ))
                .Select(x => x.Name)
                .Distinct()
                .ToArray();
        }
    }
}

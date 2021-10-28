using System;
using System.Text.Json;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz
{
    internal static class JsonElementObjectConverter
    {
        internal static object ConvertToNetObject(this JsonElement jsonElement)
        {
            var kind = jsonElement.ValueKind;           

            return kind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.GetDouble(),
                JsonValueKind.Array => ConvertToArray(jsonElement),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Object => ConvertToDynamicObject(jsonElement),
                _ => throw new Exception("Cannot convert JsonElement to known object.")
            };
        }

        private static object ConvertToArray(JsonElement jsonElement)
        {
            var len = jsonElement.GetArrayLength();
            var result = new object[len];
            var i = 0;
            foreach (var el in jsonElement.EnumerateArray())
            {
                result[i++] = el.ConvertToNetObject();
            }
            return result;
        }

        private static CaseSensitiveDynamicObject ConvertToDynamicObject(JsonElement jsonElement)
        {
            var obj = new CaseSensitiveDynamicObject();
            foreach (var el in jsonElement.EnumerateObject())
            {
                obj[el.Name] = ConvertToNetObject(el.Value);
            }
            return obj;
        }
    }
}

using System;
using System.Text.Json;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.API
{
    internal static class JsonElementConcurrentJsObjectConverter
    {
        internal static object ConvertToConcurrentJsObject(this JsonElement jsonElement)
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

        private static object ConvertToArray(
            JsonElement jsonElement)
        {
            var result = new JsArray();
            var i = 0;
            foreach (var el in jsonElement.EnumerateArray())
            {
                result[i++] = el.ConvertToConcurrentJsObject();
            }
            return result;
        }

        private static ConcurrentJsObject ConvertToDynamicObject(
            JsonElement jsonElement)
        {
            var obj = new ConcurrentJsObject();
            foreach (var el in jsonElement.EnumerateObject())
            {
                obj[el.Name] = ConvertToConcurrentJsObject(el.Value);
            }
            return obj;
        }
    }
}

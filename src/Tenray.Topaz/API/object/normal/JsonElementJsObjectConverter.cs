using System;
using System.Text.Json;

namespace Tenray.Topaz.API
{
    internal static class JsonElementJsObjectConverter
    {
        internal static object ConvertToJsObject(this JsonElement jsonElement)
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
            var result = new JsArray();
            var i = 0;
            foreach (var el in jsonElement.EnumerateArray())
            {
                result[i++] = el.ConvertToJsObject();
            }
            return result;
        }

        private static JsObject ConvertToDynamicObject(JsonElement jsonElement)
        {
            var obj = new JsObject();
            foreach (var el in jsonElement.EnumerateObject())
            {
                obj[el.Name] = ConvertToJsObject(el.Value);
            }
            return obj;
        }
    }
}

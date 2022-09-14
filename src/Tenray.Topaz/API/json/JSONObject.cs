using System.Text.Encodings.Web;
using System.Text.Json;

namespace Tenray.Topaz.API
{
    public sealed class JSONObject : JsObject
    {
        public JsonSerializerOptions Options { get; set; } = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
#if NET6_0_OR_GREATER
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
#endif
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        public JsonSerializerOptions OptionsIndented { get; set; } = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
#if NET6_0_OR_GREATER
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
#endif
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public string stringify(object value, object replacer = null, int space = 0)
        {
            return JsonSerializer.Serialize(value, space == 0 ? Options : OptionsIndented);
        }

        public object parse(string json)
        {
            return JsonSerializer.Deserialize<JsonElement>(json, Options).ConvertToJsObject();
        }
    }
}

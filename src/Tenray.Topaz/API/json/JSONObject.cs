using Microsoft.Collections.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.API
{
    public class JSONObject : JsObject
    {
        public JsonSerializerOptions Options { get; set; } = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        public string stringify(object value)
        {
            return JsonSerializer.Serialize(value, Options);
        }

        public object parse(string json)
        {
            return JsonSerializer.Deserialize<JsonElement>(json, Options).ConvertToJsObject();
        }

    }
}

using System;

namespace Tenray.Topaz.API;

[Flags]
public enum JsObjectConverterOption
{
    None,
    UseLowerCasePropertyNames,
    CreateConcurrentJsObject
}

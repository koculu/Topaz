using System.Reflection;

namespace Tenray.Topaz.API;

public static class JsObjectConverter
{
    /// <summary>
    /// Creates a new IJsObject by using all public instance properties of the given source object.
    /// If the source object is IJsObject returns the source object.
    /// By default the property names are converted to lowercase to match Javascript common code style.
    /// </summary>
    /// <param name="sourceObject">The source object</param>
    /// <param name="option">JsObject creation option</param>
    /// <returns></returns>
    public static IJsObject ToJsObject(this object sourceObject,
        JsObjectConverterOption option = JsObjectConverterOption.UseLowerCasePropertyNames)
    {
        if (sourceObject == null)
            return null;
        if (sourceObject is IJsObject jsObject)
            return jsObject;
        jsObject = option.HasFlag(JsObjectConverterOption.CreateConcurrentJsObject) ?
            new ConcurrentJsObject()
            : new JsObject();
        var type = sourceObject.GetType();
        var props = type.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.GetProperty);
        var useLowerCasePropertyNames = option.HasFlag(JsObjectConverterOption.UseLowerCasePropertyNames);
        foreach (var prop in props)
        {
            var value = prop.GetValue(sourceObject);
            var propertyName = useLowerCasePropertyNames ?
                char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..] :
                prop.Name;
            jsObject.SetValue(propertyName, value);
        }
        return jsObject;
    }
}

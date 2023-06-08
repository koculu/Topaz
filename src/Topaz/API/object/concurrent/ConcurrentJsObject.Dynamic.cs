using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace Tenray.Topaz.API;

public partial class ConcurrentJsObject : DynamicObject
{
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        SetValue(binder.Name, value);
        return true;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        return TryGetValue(binder.Name, out result);
    }

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return dictionary.Keys.ToArray();
    }

    protected static object ConvertJsonElementToConcurrentJsObject(object value)
    {
        if (value == null)
            return null;
        if (value is JsonElement jsonElement)
            return jsonElement.ConvertToConcurrentJsObject();
        return value;
    }
}

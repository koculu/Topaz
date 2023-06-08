using System;
using System.Collections;
using System.Reflection;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.API;

public interface IJsObject
{
    bool TryGetValue(object key, out object value);

    IEnumerable GetObjectKeys();

    void SetValue(object key, object value);

    internal void UnwrapObject(ScriptExecutor scriptExecutor);

    internal bool IsPrototypeProperty(object member);
}
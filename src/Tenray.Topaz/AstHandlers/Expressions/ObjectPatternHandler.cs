using Esprima.Ast;
using System;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Expressions
{
    internal static class ObjectPatternHandler
    {
        // https://hacks.mozilla.org/2015/05/es6-in-depth-destructuring/
        internal static object ProcessObjectPattern(
            ScriptExecutor scriptExecutor,
            ObjectPattern objectPattern,
            object value,
            Action<object, object> callback)
        {
            var scope = scriptExecutor;
            var props = objectPattern.Properties;
            var len = props.Count;
            for (var i = 0; i < len; ++i)
            {
                var p = props[i];
                if (p is RestElement restElement)
                {
                    var arg = scriptExecutor.ExecuteStatement(restElement.Argument);
                    if (arg is TopazIdentifier topazIdentifier)
                    {
                        if (DynamicHelper
                            .TryGetDynamicMemberValue(
                                value,
                                topazIdentifier.Name,
                                out var item))
                            callback(arg, item);
                    }
                    continue;
                }
                if (p is Property prop)
                {
                    var left = scriptExecutor.ExecuteStatement(prop.Key);
                    var key = left;
                    if (prop.Computed)
                        key = scriptExecutor.GetValue(key);
                    else if (key is TopazIdentifier identifier)
                        key = identifier.Name;
                    var keyString = key.ToString();
                    if (keyString == null)
                        Exceptions.ThrowNullReferenceException("Object destructor property key is null.");
                    object defaultValue = null;
                    if (prop.Kind == PropertyKind.Init || prop.Kind == PropertyKind.Data)
                    {
                        if (prop.Value is ArrayPattern nestedArrayPattern)
                        {
                            if (!DynamicHelper
                                .TryGetDynamicMemberValue(
                                value, keyString, out var nestedValue))
                                continue;

                            ArrayPatternHandler.ProcessArrayPattern(
                                scriptExecutor,
                                nestedArrayPattern,
                                nestedValue,
                                callback);
                            continue;
                        }
                        else if (prop.Value is ObjectPattern nestedObjectPattern)
                        {
                            if (!DynamicHelper
                                .TryGetDynamicMemberValue(
                                value, keyString, out var nestedValue))
                                continue;
                            ProcessObjectPattern(
                                scriptExecutor,
                                nestedObjectPattern,
                                nestedValue,
                                callback);
                            continue;
                        }
                        if (prop.Value is AssignmentPattern assignmentPattern)
                        {
                            defaultValue = scriptExecutor
                                .ExecuteExpressionAndGetValue(assignmentPattern.Right);
                        }
                    }
                    if (DynamicHelper
                        .TryGetDynamicMemberValue(value, keyString, out var item))
                        callback(left, item);
                    else
                        callback(left, defaultValue);
                }
            }
            return value;
        }
    }
}

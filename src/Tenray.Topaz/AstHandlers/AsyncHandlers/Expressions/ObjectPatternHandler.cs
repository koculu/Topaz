using Esprima.Ast;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions;

internal static partial class ObjectPatternHandler
{
    // https://hacks.mozilla.org/2015/05/es6-in-depth-destructuring/
    internal async static ValueTask<object> ProcessObjectPatternAsync(
        ScriptExecutor scriptExecutor,
        ObjectPattern objectPattern,
        object value,
        Func<object, object, CancellationToken, ValueTask> callback,
        CancellationToken token)
    {
        var topazEngine = scriptExecutor.TopazEngine;
        var scope = scriptExecutor;
        var props = objectPattern.Properties;
        var len = props.Count;
        for (var i = 0; i < len; ++i)
        {
            token.ThrowIfCancellationRequested();
            var p = props[i];
            if (p is RestElement restElement)
            {
                var arg = await scriptExecutor
                    .ExecuteStatementAsync(restElement.Argument, token);
                if (arg is TopazIdentifier topazIdentifier)
                {
                    topazIdentifier.InvalidateLocalCache();
                    if (topazEngine
                        .TryGetObjectMember(value, topazIdentifier.Name,
                        out var item, false))
                        await callback(arg, item, token);
                }
                continue;
            }
            if (p is Property prop)
            {
                var left = await scriptExecutor.ExecuteStatementAsync(prop.Key, token);
                var key = left;
                if (prop.Computed)
                    key = scriptExecutor.GetValue(key);
                else if (key is TopazIdentifier topazIdentifier)
                {
                    topazIdentifier.InvalidateLocalCache();
                    key = topazIdentifier.Name;
                }
                var keyString = key.ToString();
                if (keyString == null)
                    Exceptions.ThrowNullReferenceException("Object destructor property key is null.");
                object defaultValue = null;
                if (prop.Kind == PropertyKind.Init || prop.Kind == PropertyKind.Data)
                {
                    if (prop.Value is ArrayPattern nestedArrayPattern)
                    {
                        if (!topazEngine
                            .TryGetObjectMember(
                            value, keyString, out var nestedValue))
                            continue;

                        await ArrayPatternHandler.ProcessArrayPatternAsync(
                            scriptExecutor,
                            nestedArrayPattern,
                            nestedValue,
                            callback,
                            token);
                        continue;
                    }
                    else if (prop.Value is ObjectPattern nestedObjectPattern)
                    {
                        if (!topazEngine
                            .TryGetObjectMember(
                            value, keyString, out var nestedValue))
                            continue;
                        await ProcessObjectPatternAsync(
                            scriptExecutor,
                            nestedObjectPattern,
                            nestedValue,
                            callback,
                            token);
                        continue;
                    }
                    if (prop.Value is AssignmentPattern assignmentPattern)
                    {
                        defaultValue = await scriptExecutor
                            .ExecuteExpressionAndGetValueAsync(assignmentPattern.Right, token);
                    }
                }
                if (topazEngine
                    .TryGetObjectMember(value, keyString, out var item))
                    await callback(left, item, token);
                else
                    await callback(left, defaultValue, token);
            }
        }
        return value;
    }
}

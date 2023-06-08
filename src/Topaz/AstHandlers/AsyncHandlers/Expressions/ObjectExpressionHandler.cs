using Esprima.Ast;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.API;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz.Expressions;

internal static partial class ObjectExpressionHandler
{
    internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
    {
        var expr = (ObjectExpression)expression;
        var props = expr.Properties;
        var len = props.Count;
        var engine = scriptExecutor.TopazEngine;
        var obj = engine.Options.UseThreadSafeJsObjects ?
                (IJsObject)new ConcurrentJsObject() :
                new JsObject();
        for (var i = 0; i < len; ++i)
        {
            var item = props[i];
            if (item is Property prop)
            {
                var key = await scriptExecutor.ExecuteStatementAsync(prop.Key, token);
                if (prop.Computed)
                    key = scriptExecutor.GetValue(key);
                else if (key is TopazIdentifier topazIdentifier)
                {
                    topazIdentifier.InvalidateLocalCache();
                    key = topazIdentifier.Name;
                }
                if (prop.Kind == PropertyKind.Init || prop.Kind == PropertyKind.Data)
                {
                    var value =
                        await scriptExecutor.ExecuteStatementAsync(prop.Value, token);
                    obj.SetValue(key, value);
                }
            }
            else if (item is SpreadElement spreadElement)
            {
                var value = await scriptExecutor
                    .ExecuteStatementAsync(spreadElement.Argument, token);
                if (value is not IEnumerable enumerable)
                {
                    return Exceptions.ThrowValueIsNotEnumerable(value);
                }
                var j = 0;
                foreach (var el in enumerable)
                {
                    token.ThrowIfCancellationRequested();
                    obj.SetValue(j, el);
                    ++j;
                }
                continue;
            }
        }
        // We cannot return salt object or
        // unwrap here as assignment pattern can define variables
        // in an object.
        // Unwrap will happen in the first GetValue statement once.
        // And this is the stage where the object is actually used in an expression.
        return new TopazObjectWrapper(scriptExecutor, obj);
    }
}

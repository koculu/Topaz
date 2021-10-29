using Esprima.Ast;
using System.Collections;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Expressions
{
    internal static partial class ObjectExpressionHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression)
        {
            var expr = (ObjectExpression)expression;
            var props = expr.Properties;
            var len = props.Count;
            var obj = new CaseSensitiveDynamicObject();
            for (var i = 0; i < len; ++i)
            {
                var item = props[i];
                if (item is Property prop)
                {
                    var key = await scriptExecutor.ExecuteStatementAsync(prop.Key);
                    if (prop.Computed)
                        key = scriptExecutor.GetValue(key);
                    else if (key is TopazIdentifier identifier)
                        key = identifier.Name;
                    if (prop.Kind == PropertyKind.Init || prop.Kind == PropertyKind.Data)
                    {
                        var value =
                            await scriptExecutor.ExecuteStatementAsync(prop.Value);
                        obj[key.ToString()] = value;
                    }
                }
                else if (item is SpreadElement spreadElement)
                {
                    var value = await scriptExecutor
                        .ExecuteStatementAsync(spreadElement.Argument);
                    if (value is not IEnumerable enumerable)
                    {
                        return Exceptions.ThrowValueIsNotEnumerable(value);
                    }
                    var j = 0;
                    foreach (var el in enumerable)
                    {
                        obj[j.ToString()] = el;
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
}

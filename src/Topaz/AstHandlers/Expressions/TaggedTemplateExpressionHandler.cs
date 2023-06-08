using Esprima.Ast;
using System.Collections.Generic;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class TaggedTemplateExpressionHandler
{
    internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
    {
        var tagged = (TaggedTemplateExpression)expression;
        var tag = tagged.Tag;
        var literal = tagged.Quasi;
        var quasis = literal.Quasis;
        var list = literal.Expressions;
        var quasisLen = quasis.Count;
        var listLen = list.Count;
        var strings = new List<object>();
        for (var i = 0; i < quasisLen; i++)
        {
            var quasi = quasis[i];
            strings.Add(quasi.Value.Cooked);
        }
        var args = new List<object>();
        args.Add(strings);
        for (var i = 0; i < listLen; ++i)
        {
            args.Add(scriptExecutor.ExecuteExpressionAndGetValue(list[i], token));
        }
        var callee = scriptExecutor.ExecuteExpressionAndGetValue(tag, token);
        return scriptExecutor.CallFunction(callee, args.ToArray(), false, token);
    }
}

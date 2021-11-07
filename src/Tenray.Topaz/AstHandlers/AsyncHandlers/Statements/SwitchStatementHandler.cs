using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.Expressions;

namespace Tenray.Topaz.Statements
{
    internal static partial class SwitchStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
        {
            var expr = (SwitchStatement)statement;
            var testValue = await scriptExecutor.ExecuteExpressionAndGetValueAsync(expr.Discriminant, token);
            var cases = expr.Cases;
            var len = cases.Count;
            var matchedAnyCase = false;
            SwitchCase defaultCase = null;
            for (var i = 0; i < len; ++i)
            {
                var @case = cases[i];
                var test = @case.Test;
                if (test == null)
                {
                    defaultCase = @case;
                    continue;
                }
                var caseValue = await scriptExecutor.ExecuteExpressionAndGetValueAsync(test, token);
                var comparison = matchedAnyCase ?
                    null :
                    BinaryExpressionHandler
                    .ExecuteBinaryOperator(scriptExecutor,
                    BinaryOperator.StrictlyEqual,
                    testValue, caseValue);
                if (matchedAnyCase || JavascriptTypeUtility.IsObjectTrue(comparison))
                {
                    matchedAnyCase = true;
                    var list = @case.Consequent;
                    var jlen = list.Count;
                    for (var j = 0; j < jlen; ++j)
                    {
                        var result = await scriptExecutor.ExecuteStatementAsync(list[j], token);
                        if (result is ReturnWrapper)
                            return result;
                        if (result is BreakWrapper)
                            return scriptExecutor.GetNullOrUndefined();
                        if (result is ContinueWrapper)
                            return result;
                    }
                }
            }

            if (defaultCase != null)
            {
                var list = defaultCase.Consequent;
                var jlen = list.Count;
                for (var j = 0; j < jlen; ++j)
                {
                    var result = await scriptExecutor.ExecuteStatementAsync(list[j], token);
                    if (result is ReturnWrapper)
                        return result;
                    if (result is BreakWrapper)
                        return scriptExecutor.GetNullOrUndefined();
                    if (result is ContinueWrapper)
                        return result;
                }
            }

            return scriptExecutor.GetNullOrUndefined();
        }
    }

}

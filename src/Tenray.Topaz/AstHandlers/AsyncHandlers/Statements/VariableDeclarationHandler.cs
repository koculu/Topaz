using Esprima.Ast;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Expressions;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Statements
{
    internal static partial class VariableDeclarationHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (VariableDeclaration)statement;
            var kind = (VariableKind)expr.Kind;
            var list = expr.Declarations;
            var len = list.Count;
            var scope = scriptExecutor;
            for (var i = 0; i < len; ++i)
            {
                var declaration = list[i];
                var id = declaration.Id;
                if (id is ArrayPattern arrayPattern)
                {
                    if (declaration.Init == null)
                        Exceptions.ThrowMissingInitializerInDestructuringDeclaration();
                    var itemValue = await scriptExecutor.ExecuteStatementAsync(declaration.Init);
                    await ArrayPatternHandler.ProcessArrayPatternAsync(
                        scriptExecutor,
                        arrayPattern,
                        itemValue,
                        (x, y) =>
                        {
                            scope.DefineVariable(x, y, kind);
                            return ValueTask.CompletedTask;
                        });
                    continue;
                }
                else if (id is ObjectPattern objectPattern)
                {
                    if (declaration.Init == null)
                        Exceptions.ThrowMissingInitializerInDestructuringDeclaration();
                    var itemValue = await scriptExecutor
                        .ExecuteStatementAsync(declaration.Init);
                    await ObjectPatternHandler.ProcessObjectPatternAsync(
                        scriptExecutor,
                        objectPattern,
                        itemValue,
                        (x, y) =>
                        {
                            scope.DefineVariable(x, y, kind);
                            return ValueTask.CompletedTask;
                        });
                    continue;
                }
                var identifier = (TopazIdentifier)await scriptExecutor.ExecuteStatementAsync(id);
                var init = declaration.Init;
                object value = null;
                if (init != null)
                {
                    value = await scriptExecutor.ExecuteExpressionAndGetValueAsync(init);
                }
                scope.DefineVariable(identifier.Name, value, kind);
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

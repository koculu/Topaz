using Esprima.Ast;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Expressions;
using Tenray.Topaz.Utility;

namespace Tenray.Topaz.Statements
{
    internal static partial class VariableDeclarationHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
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
                    var itemValue = scriptExecutor
                        .ExecuteExpressionAndGetValue(declaration.Init, token);
                    ArrayPatternHandler.ProcessArrayPattern(
                        scriptExecutor,
                        arrayPattern,
                        itemValue,
                        (x, y, token) =>
                        {
                            scope.DefineVariable(x, y, kind);
                        },
                        token);
                    continue;
                }
                else if (id is ObjectPattern objectPattern)
                {
                    if (declaration.Init == null)
                        Exceptions.ThrowMissingInitializerInDestructuringDeclaration();
                    var itemValue = scriptExecutor
                        .ExecuteExpressionAndGetValue(declaration.Init, token);
                    ObjectPatternHandler.ProcessObjectPattern(
                        scriptExecutor,
                        objectPattern,
                        itemValue,
                        (x, y, token) =>
                        {
                            scope.DefineVariable(x, y, kind);
                        },
                        token);
                    continue;
                }
                var identifier = (TopazIdentifier)scriptExecutor.ExecuteStatement(id, token);
                var init = declaration.Init;
                object value = scriptExecutor.GetNullOrUndefined();
                if (init != null)
                {
                    value = scriptExecutor.ExecuteExpressionAndGetValue(init, token);
                }
                scope.DefineVariable(identifier.Name, value, kind);
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

using Esprima.Ast;
using System;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class TryStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (TryStatement)statement;
            var block = expr.Block;
            var handler = expr.Handler;
            var finalizer = expr.Finalizer;
            try
            {
                var bodyScope = scriptExecutor.NewBlockScope();
                return bodyScope.ExecuteStatement(block);
            }
            catch (Exception e)
            {
                if (handler == null)
                    throw;

                var bodyScope = scriptExecutor.NewBlockScope();
                var paramName = (handler.Param as Identifier)?.Name;
                if (paramName != null)
                    bodyScope
                        .DefineVariable(paramName, e, VariableKind.Var);

                return bodyScope.ExecuteStatement(handler.Body);
            }
            finally
            {
                if (finalizer != null)
                {
                    var bodyScope = scriptExecutor.NewBlockScope();
                    bodyScope.ExecuteStatement(finalizer);
                }
            }
        }
    }

}

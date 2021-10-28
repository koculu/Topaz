using Esprima.Ast;
using System;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal class TryStatementHandler
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
                var paramName = (handler.Param as Identifier)?.Name;
                if (paramName != null)
                    scriptExecutor
                        .DefineVariable(paramName, e.InnerException, VariableKind.Var);

                var bodyScope = scriptExecutor.NewBlockScope();
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

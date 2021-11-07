using Esprima.Ast;
using System;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class TryStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
        {
            var expr = (TryStatement)statement;
            var block = expr.Block;
            var handler = expr.Handler;
            var finalizer = expr.Finalizer;
            try
            {
                var bodyScope = scriptExecutor.NewBlockScope();
                return bodyScope.ExecuteStatement(block, token);
            }
            catch (OperationCanceledException cancelledException)
            {
                if (cancelledException.CancellationToken == token)
                {
                    throw;
                }
                else
                {
                    if (handler == null)
                        throw;
                    return HandleCatch(scriptExecutor, handler, cancelledException, token);
                }
            }
            catch (Exception e)
            {
                if (handler == null)
                    throw;
                return HandleCatch(scriptExecutor, handler, e, token);
            }
            finally
            {
                if (finalizer != null)
                {
                    var bodyScope = scriptExecutor.NewBlockScope();
                    bodyScope.ExecuteStatement(finalizer, token);
                }
            }
        }

        private static object HandleCatch(ScriptExecutor scriptExecutor, CatchClause handler, Exception e, CancellationToken token)
        {
            var bodyScope = scriptExecutor.NewBlockScope();
            var paramName = (handler.Param as Identifier)?.Name;
            if (paramName != null)
                bodyScope
                    .DefineVariable(paramName, e, VariableKind.Var);

            return bodyScope.ExecuteStatement(handler.Body, token);
        }
    }

}

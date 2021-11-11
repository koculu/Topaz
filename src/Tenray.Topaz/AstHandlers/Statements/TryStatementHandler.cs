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
            var bodyScope = scriptExecutor.NewBlockScope();
            try
            {
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
                bodyScope.ReturnToPool();
                if (finalizer != null)
                {
                    bodyScope = scriptExecutor.NewBlockScope();
                    bodyScope.ExecuteStatement(finalizer, token);
                    bodyScope.ReturnToPool();
                }
            }
        }

        private static object HandleCatch(ScriptExecutor scriptExecutor, CatchClause handler, Exception e, CancellationToken token)
        {
            var bodyScope = scriptExecutor.NewBlockScope();
            var identifier = handler.Param as Identifier;
            bodyScope.DefineVariable(identifier, e, VariableKind.Var);
            var result = bodyScope.ExecuteStatement(handler.Body, token);
            bodyScope.ReturnToPool();
            return result;
        }
    }

}

using Esprima.Ast;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class TryStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
        {
            var expr = (TryStatement)statement;
            var block = expr.Block;
            var handler = expr.Handler;
            var finalizer = expr.Finalizer;
            var bodyScope = scriptExecutor.NewBlockScope();
            try
            {
                return await bodyScope.ExecuteStatementAsync(block, token);
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
                    return await HandleCatchAsync(scriptExecutor, handler, cancelledException, token);
                }
            }
            catch (Exception e)
            {
                if (handler == null)
                    throw;
                return await HandleCatchAsync(scriptExecutor, handler, e, token);

            }
            finally
            {
                bodyScope.ReturnToPool();
                if (finalizer != null)
                {
                    bodyScope = scriptExecutor.NewBlockScope();
                    await bodyScope.ExecuteStatementAsync(finalizer, token);
                    bodyScope.ReturnToPool();
                }
            }
        }

        private static async ValueTask<object> HandleCatchAsync(ScriptExecutor scriptExecutor, CatchClause handler, Exception e, CancellationToken token)
        {
            var bodyScope = scriptExecutor.NewBlockScope();
            var identifier = handler.Param as Identifier;
            bodyScope.DefineVariable(identifier, e, VariableKind.Var);
            var result = await bodyScope.ExecuteStatementAsync(handler.Body, token);
            bodyScope.ReturnToPool();
            return result;
        }
    }
}

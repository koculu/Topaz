using Esprima.Ast;
using System;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class TryStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (TryStatement)statement;
            var block = expr.Block;
            var handler = expr.Handler;
            var finalizer = expr.Finalizer;
            try
            {
                var bodyScope = scriptExecutor.NewBlockScope();
                return await bodyScope.ExecuteStatementAsync(block);
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

                return await bodyScope.ExecuteStatementAsync(handler.Body);
            }
            finally
            {
                if (finalizer != null)
                {
                    var bodyScope = scriptExecutor.NewBlockScope();
                    await bodyScope.ExecuteStatementAsync(finalizer);
                }
            }
        }
    }

}

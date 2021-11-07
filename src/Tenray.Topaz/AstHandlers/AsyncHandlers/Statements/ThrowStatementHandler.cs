using Esprima.Ast;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class ThrowStatementHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
        {
            var expr = (ThrowStatement)statement;
            var err = await scriptExecutor.ExecuteStatementAsync(expr.Argument, token);
            if (err is Exception e)
                throw e;
            throw new Exception(err.ToString());
        }
    }

}

using Esprima.Ast;
using System;
using System.Threading;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements;

internal static partial class ThrowStatementHandler
{
    internal static object Execute(ScriptExecutor scriptExecutor, Node statement, CancellationToken token)
    {
        var expr = (ThrowStatement)statement;
        var err = scriptExecutor.ExecuteExpressionAndGetValue(expr.Argument, token);
        if (err is Exception e)
            throw e;
        throw new Exception(err.ToString());
    }
}

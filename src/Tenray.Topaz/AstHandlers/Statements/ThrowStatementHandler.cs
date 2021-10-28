using Esprima.Ast;
using System;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal class ThrowStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ThrowStatement)statement;
            var err = scriptExecutor.ExecuteExpressionAndGetValue(expr.Argument);
            if (err is Exception e)
                throw e;
            throw new Exception(err.ToString());
        }
    }

}

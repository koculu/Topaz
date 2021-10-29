using Esprima.Ast;
using System;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Statements
{
    internal static partial class ExpressionStatementHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node statement)
        {
            var expr = (ExpressionStatement)statement;
            return scriptExecutor.ExecuteStatement(expr.Expression);
        }
    }

}

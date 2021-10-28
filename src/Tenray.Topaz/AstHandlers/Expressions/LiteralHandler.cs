using Esprima;
using Esprima.Ast;
using System;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal class LiteralHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression)
        {
            var literal = (Literal)expression;
            if (literal.TokenType == TokenType.BooleanLiteral)
            {
                return literal.NumericValue > 0.0;
            }

            if (literal.TokenType == TokenType.NullLiteral)
            {
                return null;
            }

            if (literal.TokenType == TokenType.NumericLiteral)
            {
                return literal.Value;
            }

            if (literal.TokenType == TokenType.StringLiteral)
            {
                return literal.StringValue;
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

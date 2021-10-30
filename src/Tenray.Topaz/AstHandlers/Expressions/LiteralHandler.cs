using Esprima;
using Esprima.Ast;
using System;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class LiteralHandler
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
                var value = literal.Value;
                if (value is long d)
                {
                    if (d < int.MinValue || d > int.MaxValue)
                        return d;
                    return (int)d;
                }
                return value;
            }

            if (literal.TokenType == TokenType.StringLiteral)
            {
                return literal.StringValue;
            }
            return scriptExecutor.GetNullOrUndefined();
        }
    }
}

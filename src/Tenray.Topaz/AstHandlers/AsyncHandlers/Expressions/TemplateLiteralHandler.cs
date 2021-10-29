using Esprima.Ast;
using System.Text;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class TemplateLiteralHandler
    {
        internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression)
        {
            var literal = (TemplateLiteral)expression;
            var quasis = literal.Quasis;
            var list = literal.Expressions;
            var quasisLen = quasis.Count;
            var listLen = list.Count;
            var sb = new StringBuilder();
            for (var i = 0; i < quasisLen; i++)
            {
                var quasi = quasis[i];
                sb.Append(quasi.Value.Cooked);
                if (i < listLen)
                {
                    sb.Append(await scriptExecutor.ExecuteExpressionAndGetValueAsync(list[i]));
                }
            }
            return sb.ToString();
        }
    }
}

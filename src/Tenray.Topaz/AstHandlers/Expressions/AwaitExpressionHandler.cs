using Esprima.Ast;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions
{
    internal static partial class AwaitExpressionHandler
    {
        internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
        {
            var expr = (AwaitExpression)expression;
            var result = scriptExecutor.ExecuteStatement(expr.Argument, token);
            if (result is Task task)
            {
                return GetTaskResult(task);
            }
            else if (result is ValueTask valueTask)
            {
                return GetTaskResult(valueTask.AsTask());
            }
            return result;
        }

        internal static object GetTaskResult(Task task)
        {
            task.Wait();
            var property = task.GetType().GetProperty("Result",
                BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                return null;
            return property.GetValue(task);
        }
    }
}

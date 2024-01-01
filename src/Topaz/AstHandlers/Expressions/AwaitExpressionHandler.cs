using Esprima.Ast;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class AwaitExpressionHandler
{
    internal static object Execute(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
    {
        var expr = (AwaitExpression)expression;
        var result = scriptExecutor.ExecuteExpressionAndGetValue(expr.Argument, token);
        var awaitHandler = scriptExecutor.TopazEngine.AwaitExpressionHandler;
        if (result == null)
            return null;
        if (awaitHandler != null)
        {
            var task1 = awaitHandler.HandleAwaitExpression(result);
            task1.Wait(token);
            return task1.Result;
        }
        if (result is Task task)
        {
            task.Wait(token);
            var type = task.GetType();
            var returnType = type.GetMethod("GetAwaiter").ReturnType.GetMethod("GetResult").ReturnType;
            if (returnType != typeof(void) && returnType.FullName != "System.Threading.Tasks.VoidTaskResult")
                return GetTaskResult(task);
            return null;
        }

        if (result is ValueTask valueTask)
        {
            valueTask.AsTask().Wait(token);
            return null;
        }

        var type2 = result.GetType();
        if (type2.IsGenericType && typeof(ValueTask<>) == type2.GetGenericTypeDefinition())
        {
            dynamic r = result;
            r.AsTask().Wait(token);
            return r.Result;
        }
        return result;
    }

    internal static object GetTaskResult(Task task)
    {
        var property = task.GetType().GetProperty("Result",
            BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
            return null;
        return property.GetValue(task);
    }
}

using Esprima.Ast;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;

namespace Tenray.Topaz.Expressions;

internal static partial class AwaitExpressionHandler
{
    internal async static ValueTask<object> ExecuteAsync(ScriptExecutor scriptExecutor, Node expression, CancellationToken token)
    {
        var expr = (AwaitExpression)expression;
        var result = await scriptExecutor.ExecuteExpressionAndGetValueAsync(expr.Argument, token);
        if (result == null)
            return null;
        var awaitHandler = scriptExecutor.TopazEngine.AwaitExpressionHandler;
        if (awaitHandler != null)
        {
            return await awaitHandler.HandleAwaitExpression(result);
        }
        if (result is Task task)
        {
            var type = task.GetType();
            var returnType = type.GetMethod("GetAwaiter").ReturnType.GetMethod("GetResult").ReturnType;
            if (returnType != typeof(void) && returnType.FullName != "System.Threading.Tasks.VoidTaskResult")
                return await (dynamic)task;
            await task;
            return null;
        }

        if (result is ValueTask valueTask)
        {
            await valueTask;
            return null;
        }

        var type2 = result.GetType();
        if (type2.IsGenericType && typeof(ValueTask<>) == type2.GetGenericTypeDefinition())
        {
            dynamic awaitable = result;
            if (result.GetType().IsGenericType)
                return await awaitable;
            await awaitable;
            return null;
        }
        return result;
    }
}

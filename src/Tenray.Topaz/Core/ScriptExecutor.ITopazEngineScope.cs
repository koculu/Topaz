using Esprima;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tenray.Topaz.Core
{
    internal partial class ScriptExecutor : ITopazEngineScope
    {
        bool ITopazEngineScope.IsThreadSafe => IsThreadSafeScope;

        bool ITopazEngineScope.IsReadOnly { get => IsReadOnly; set => IsReadOnly = value; }

        bool ITopazEngineScope.IsFrozen { get => IsFrozen; set => IsFrozen = value; }

        bool ITopazEngineScope.IsGlobalScope => ScopeType == ScopeType.Global;

        void ITopazEngineScope.ExecuteScript(string code, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(code))
                return;
            var script = new JavaScriptParser(code, Options.ParserOptions)
                .ParseScript();
            ExecuteScript(script, token);
        }

        object ITopazEngineScope.ExecuteExpression(string code, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            var script = new JavaScriptParser(code, Options.ParserOptions)
                .ParseExpression();
            return ExecuteExpressionAndGetValue(script, token);
        }

        object ITopazEngineScope.InvokeFunction(string name, CancellationToken token, params object[] args)
        {
            return CallFunction(
                new TopazIdentifier(name),
                args.ToArray(), false, token);
        }

        object ITopazEngineScope.InvokeFunction(object functionObject, CancellationToken token, params object[] args)
        {
            return CallFunction(functionObject, args, false, token);
        }

        ITopazEngineScope ITopazEngineScope.NewChildScope(bool? isThreadSafe)
        {
            return NewCustomScope(isThreadSafe);
        }

        object ITopazEngineScope.GetValue(string name)
        {
            return GetVariableValue(name);
        }

        void ITopazEngineScope.SetValue(string name, object value)
        {
            AddOrUpdateVariableValueInTheScope(name, value, VariableKind.Var);
        }

        void ITopazEngineScope.SetValueAndKind(
            string name, object value, VariableKind variableKind)
        {
            AddOrUpdateVariableValueAndKindInTheScope
                   (name, value, variableKind);
        }

        async Task ITopazEngineScope.ExecuteScriptAsync(string code, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(code))
                return;
            var script = new JavaScriptParser(code, Options.ParserOptions)
                .ParseScript();
            await ExecuteScriptAsync(script, token);
        }

        async Task<object> ITopazEngineScope.ExecuteExpressionAsync(string code, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            var script = new JavaScriptParser(code, Options.ParserOptions)
                .ParseExpression();
            return await ExecuteExpressionAndGetValueAsync(script, token);
        }

        async Task<object> ITopazEngineScope.InvokeFunctionAsync(string name, CancellationToken token, params object[] args)
        {
            return await CallFunctionAsync(
                new TopazIdentifier(name),
                args.ToArray(), false, token);
        }

        async Task<object> ITopazEngineScope.InvokeFunctionAsync(object functionObject, CancellationToken token, params object[] args)
        {
            return await CallFunctionAsync(functionObject, args, false, token);
        }
    }
}

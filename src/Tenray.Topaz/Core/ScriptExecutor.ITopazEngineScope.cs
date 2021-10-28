using Esprima;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tenray.Topaz.Core
{
    internal partial class ScriptExecutor : ITopazEngineScope
    {
        bool ITopazEngineScope.IsReadOnly { get => IsReadOnly; set => IsReadOnly = value; }

        bool ITopazEngineScope.IsFrozen { get => IsFrozen; set => IsFrozen = value; }

        bool ITopazEngineScope.IsGlobalScope => ScopeType == ScopeType.Global;

        void ITopazEngineScope.ExecuteScript(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return;
            var script = new JavaScriptParser(code, Options.ParserOptions)
                .ParseScript();
            ExecuteScript(script);
        }

        object ITopazEngineScope.ExecuteExpression(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            var script = new JavaScriptParser(code, Options.ParserOptions)
                .ParseExpression();
            return ExecuteExpressionAndGetValue(script);
        }

        object ITopazEngineScope.InvokeFunction(string name, params object[] args)
        {
            return CallFunction(
                new TopazIdentifier(this, name),
                args.ToArray(), false);
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
    }
}

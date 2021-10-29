using Esprima;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tenray.Topaz.Core;
using Tenray.Topaz.Options;

namespace Tenray.Topaz
{
    public class TopazEngine : ITopazEngine
    {
        private static int lastTopazEngineId = 0;

        private readonly ScriptExecutor globalScope;

        public int Id { get; }

        public TopazEngineOptions Options { get; set; }

        public ITopazEngineScope GlobalScope => globalScope;

        public TopazEngine(bool isThreadSafeEngine = true, 
            TopazEngineOptions options = null)
        {
            Id = Interlocked.Increment(ref lastTopazEngineId);
            globalScope = new ScriptExecutor(this, isThreadSafeEngine);
            Options = options ?? PresetOptions.FriendlyStyle;
        }

        public void ExecuteScript(string code)
        {
            GlobalScope.ExecuteScript(code);
        }

        public object ExecuteExpression(string code)
        {
            return GlobalScope.ExecuteExpression(code);
        }

        public object InvokeFunction(string name, params object[] args)
        {
            return GlobalScope.InvokeFunction(name, args);
        }

        public object InvokeFunction(object functionObject, params object[] args)
        {
            return GlobalScope.InvokeFunction(functionObject, args);
        }

        public void AddType<T>(
            string name = null,
            Action<CallType, object[]> argsConverter = null,
            VariableKind variableKind = VariableKind.Const,
            TypeOptions typeOptions = TypeOptions.Default)
        {
            AddType(typeof(T), name, argsConverter, variableKind, typeOptions);
        }

        public void AddType(
            Type type,
            string name = null,
            Action<CallType, object[]> argsConverter = null,
            VariableKind variableKind = VariableKind.Const,
            TypeOptions typeOptions = TypeOptions.Default)
        {
            GlobalScope.SetValueAndKind(name ?? type.FullName,
                new TypeWrapper(type, name, typeOptions, argsConverter),
                variableKind);
        }

        public object GetValue(string name)
        {
            return GlobalScope.GetValue(name);
        }
        
        public void SetValue(string name, object value)
        {
            GlobalScope.SetValue(name, value);
        }

        public void SetValueAndKind(string name, object value, VariableKind variableKind)
        {
            GlobalScope.SetValueAndKind(name, value, variableKind);
        }
    }
}

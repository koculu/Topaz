using Esprima;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;
using Tenray.Topaz.Interop;
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

        public IObjectProxyRegistry ObjectProxyRegistry { get; }

        public IObjectProxy DefaultObjectProxy { get; }

        public IDelegateInvoker DelegateInvoker { get; }

        public IMemberAccessPolicy MemberAccessPolicy { get; }

        public TopazEngine(bool isThreadSafeEngine = true,
            TopazEngineOptions options = null,
            IObjectProxyRegistry objectProxyRegistry = null,
            IObjectProxy defaultObjectProxy = null,
            IDelegateInvoker delegateInvoker = null,
            IMemberAccessPolicy memberAccessPolicy = null)
        {
            Id = Interlocked.Increment(ref lastTopazEngineId);
            globalScope = new ScriptExecutor(this, isThreadSafeEngine);
            Options = options ?? PresetOptions.FriendlyStyle;
            ObjectProxyRegistry = objectProxyRegistry ?? new DictionaryObjectProxyRegistry();
            DefaultObjectProxy = defaultObjectProxy ?? new ObjectProxyUsingReflection(null);
            DelegateInvoker = delegateInvoker ?? new DelegateInvoker();
            MemberAccessPolicy = memberAccessPolicy ?? new DefaultMemberAccessPolicy(this);
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

        public void AddType<T>(string name = null, ITypeProxy typeProxy = null)
        {
            AddType(typeof(T), name, typeProxy);
        }

        public void AddType(Type type, string name = null, ITypeProxy typeProxy = null)
        {
            GlobalScope.SetValueAndKind(
                name ?? type.FullName,
                typeProxy ?? new TypeProxyUsingReflection(type, name),
                VariableKind.Const);
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

        public async Task ExecuteScriptAsync(string code)
        {
            await GlobalScope.ExecuteScriptAsync(code);
        }

        public async Task<object> ExecuteExpressionAsync(string code)
        {
            return await GlobalScope.ExecuteExpressionAsync(code);
        }

        public async Task<object> InvokeFunctionAsync(string name, params object[] args)
        {
            return await GlobalScope.InvokeFunctionAsync(name, args);
        }

        public async Task<object> InvokeFunctionAsync(object functionObject, params object[] args)
        {
            return await GlobalScope.InvokeFunctionAsync(functionObject, args);
        }

        internal bool TryGetObjectMember(
            object instance,
            object member,
            out object value,
            bool isIndexedProperty = false)
        {
            if (instance == null)
            {
                value = Options.NoUndefined ? null : Undefined.Value;
                return false;
            }

            ProcessObjectMemberSecurityPolicy(instance, member);

            if (ObjectProxyRegistry
                    .TryGetObjectProxy(instance.GetType(), out var proxy) &&
                proxy
                    .TryGetObjectMember(instance, member,
                        out value, isIndexedProperty))
                return true;

            return DefaultObjectProxy
                .TryGetObjectMember(instance, member, out value, isIndexedProperty);
        }

        internal bool TrySetObjectMember(
            object instance,
            object member,
            object value,
            bool isIndexedProperty = false)
        {
            if (instance == null)
                return false;

            ProcessObjectMemberSecurityPolicy(instance, member);

            if (ObjectProxyRegistry
                    .TryGetObjectProxy(instance, out var proxy) &&
                proxy
                    .TrySetObjectMember(instance, member,
                        value, isIndexedProperty))
                return true;

            return DefaultObjectProxy
                .TrySetObjectMember(instance, member, value, isIndexedProperty);
        }

        internal void ProcessObjectMemberSecurityPolicy(object obj, object member)
        {
            if (obj == null || member == null)
                return;
            var disableReflection =
                !Options.SecurityPolicy.HasFlag(SecurityPolicy.EnableReflection);
            if (disableReflection &&
                obj.GetType().Namespace.StartsWith("System.Reflection"))
            {
                Exceptions.ThrowReflectionSecurityException(obj, member);
            }

            if (MemberAccessPolicy
                .IsObjectMemberAccessAllowed(obj, member.ToString()))
                return;

            Exceptions.ThrowMemberAccessSecurityException(obj, member);
        }
    }
}

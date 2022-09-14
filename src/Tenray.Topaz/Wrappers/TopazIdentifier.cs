using System;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz
{
    internal sealed class TopazIdentifier
    {
        private sealed class CacheEntry
        {
            public static CacheEntry EmptyEntry = new CacheEntry(null, -1);
            
            public Variable Variable;

            public int ScopeId;

            public CacheEntry(Variable variable, int scopeId)
            {
                Variable = variable;
                ScopeId = scopeId;
            }
        }

        internal string Name { get; }

        /// <summary>
        /// Local variable cache. Reuse if available.
        /// </summary>
        CacheEntry Cache;

        internal TopazIdentifier(string name)
        {
            Name = name;
            Cache = CacheEntry.EmptyEntry;
        }

        public override string ToString()
        {
            return Name;
        }

        internal void InvalidateLocalCache()
        {
            Cache = CacheEntry.EmptyEntry;
        }

        internal object GetVariableValue(ScriptExecutor scriptExecutor)
        {
            var scopeId = scriptExecutor.Id;
            var cache = Cache;
            object value = null;
            if (cache.ScopeId == scopeId)
            {
                value = cache.Variable.Value;
            }
            else
            {
                var variable = scriptExecutor.GetVariable(Name);
                if (variable != null)
                {
                    Cache = new CacheEntry(variable, scopeId);
                    value = variable.Value;
                }
            }
            if (value == null)
            {
                var options = scriptExecutor.Options;
                if (options.AllowUndefinedReferenceAccess)
                    return options.NoUndefined ? null : Undefined.Value;
                Exceptions.ThrowVariableIsNotDefined(Name);
                return null;
            }
            return value;
        }

        internal void SetVariableValue(
            ScriptExecutor scriptExecutor,
            object value)
        {
            var scopeId = scriptExecutor.Id;
            var cache = Cache;
            if (cache.ScopeId == scopeId && !cache.Variable.IsCaptured)
            {
                cache.Variable.Value = value;
                return;
            }
            scriptExecutor.SetVariableValue(Name, value);
        }
    }
}

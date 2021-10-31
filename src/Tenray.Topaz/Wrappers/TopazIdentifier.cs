using System;
using Tenray.Topaz.Core;
using Tenray.Topaz.ErrorHandling;

namespace Tenray.Topaz
{
    internal class TopazIdentifier
    {
        internal string Name { get; }

        /// <summary>
        /// Local variable cache. Reuse if available.
        /// </summary>
        (Variable, int) Cache;

        internal TopazIdentifier(string name)
        {
            Name = name;
            Cache = (null, -1);
        }

        public override string ToString()
        {
            return Name;
        }

        internal object GetVariableValue(ScriptExecutor scriptExecutor)
        {
            var scopeId = scriptExecutor.Id;
            var (cache, scope) = Cache;
            object value = null;
            if (scope == scopeId)
            {
                value = cache?.Value;
            }
            else
            {
                var variable = scriptExecutor.GetVariable(Name);
                if (variable != null)
                {
                    Cache = (variable, scopeId);
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
    }
}

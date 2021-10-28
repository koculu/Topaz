using Tenray.Topaz.Core;

namespace Tenray.Topaz
{
    internal class TopazIdentifier
    {
        internal ScriptExecutor ScriptExecutor { get; }

        internal string Name { get; }

        internal TopazIdentifier(ScriptExecutor scriptExecutor, string name)
        {
            ScriptExecutor = scriptExecutor;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

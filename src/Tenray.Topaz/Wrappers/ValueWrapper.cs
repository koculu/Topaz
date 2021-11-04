using Esprima.Ast;
using Esprima.Utils;

namespace Tenray.Topaz
{
    internal class ValueWrapper : Expression
    {
        public override NodeCollection ChildNodes => NodeCollection.Empty;

        public readonly object Value;

        public ValueWrapper(object value) : base(Nodes.ValueWrapper)
        {
            Value = value;
        }

        protected internal override void Accept(AstVisitor visitor)
        {
        }
    }
}

using Esprima.Utils;
using Tenray.Topaz;

namespace Esprima.Ast;

public sealed class Identifier : Expression
{
    public readonly string? Name;
    internal TopazIdentifier TopazIdentifier;

    public Identifier(string? name) : base(Nodes.Identifier)
    {
        Name = name;
        TopazIdentifier = new TopazIdentifier(Name);
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;        

    protected internal override void Accept(AstVisitor visitor)
    {
        visitor.VisitIdentifier(this);
    }
}

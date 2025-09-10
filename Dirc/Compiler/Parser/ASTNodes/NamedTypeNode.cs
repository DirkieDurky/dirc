using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class NamedTypeNode : TypeNode
{
    public NamedTypeNode(Token identifierToken, string typeName) : base(identifierToken, typeName)
    {
    }

    public override string ToString() => $"NamedType({Name})";
    public override IEnumerable<AstNode> GetChildNodes() => [];
}

using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class IdentifierNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    public IdentifierNode(Token identifierToken, string name)
    {
        IdentifierToken = identifierToken;
        Name = name;
    }
    public override string ToString() => $"Identifier({Name})";
    public override IEnumerable<AstNode> GetChildNodes() => [];
}

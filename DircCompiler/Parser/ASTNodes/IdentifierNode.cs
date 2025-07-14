using Dirc.Lexing;

namespace Dirc.Parsing;

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
}

using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public abstract class TypeNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    public TypeNode(Token identifierToken, string name)
    {
        IdentifierToken = identifierToken;
        Name = name;
    }
    public override string ToString() => $"Type({Name})";
}

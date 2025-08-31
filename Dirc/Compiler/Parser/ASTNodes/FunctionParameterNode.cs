using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class FunctionParameterNode
{
    public Token IdentifierToken { get; }
    public TypeNode Type { get; }
    public string Name { get; }
    public FunctionParameterNode(Token identifierToken, TypeNode type, string name)
    {
        IdentifierToken = identifierToken;
        Type = type;
        Name = name;
    }
    public override string ToString() => $"{Type} {Name}";
}

using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public abstract class TypeNode : AstNode
{
    public Token IdentifierToken { get; }
    public string TypeName { get; }
    public TypeNode(Token identifierToken, string typeName)
    {
        IdentifierToken = identifierToken;
        TypeName = typeName;
    }
    public override string ToString() => $"Type({TypeName})";
}

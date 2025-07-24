using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class NamedTypeNode : TypeNode
{
    public NamedTypeNode(Token identifierToken, string typeName) : base(identifierToken, typeName)
    {
    }

    public override string ToString() => $"Type({TypeName})";
}

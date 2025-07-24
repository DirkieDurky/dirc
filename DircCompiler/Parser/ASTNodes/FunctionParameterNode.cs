using DircCompiler.Lexing;

namespace DircCompiler;

public class FunctionParameterNode
{
    public Token IdentifierToken { get; }
    public string TypeName { get; }
    public string Name { get; }
    public FunctionParameterNode(Token identifierToken, string typeName, string name)
    {
        IdentifierToken = identifierToken;
        TypeName = typeName;
        Name = name;
    }
    public override string ToString() => $"{TypeName} {Name}";
}

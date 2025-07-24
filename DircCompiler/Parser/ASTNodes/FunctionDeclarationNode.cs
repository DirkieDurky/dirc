using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class FunctionDeclarationNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    public TypeNode ReturnType { get; }
    public string ReturnTypeName => ReturnType.TypeName;
    public List<FunctionParameterNode> Parameters { get; }
    public List<AstNode> Body { get; }
    public FunctionDeclarationNode(Token identifierToken, string name, TypeNode returnType, List<FunctionParameterNode> parameters, List<AstNode> body)
    {
        IdentifierToken = identifierToken;
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Body = body;
    }
    public override string ToString() => $"FunctionDeclaration({ReturnTypeName} {Name}, [{string.Join(", ", Parameters)}], [\n  {string.Join(",\n  ", Body)}\n])";
}

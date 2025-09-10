using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class FunctionDeclarationNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    public TypeNode ReturnType { get; }
    public string ReturnTypeName => ReturnType.Name;
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
    public override string ToString() => $"FunctionDeclaration({ReturnType} {Name}, [{string.Join(", ", Parameters)}], [\n  {string.Join(",\n  ", Body)}\n])";

    public override IEnumerable<AstNode> GetChildNodes() => Parameters.Concat(Body).Append(ReturnType);
}

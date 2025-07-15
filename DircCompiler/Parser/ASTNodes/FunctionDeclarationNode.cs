using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class FunctionDeclarationNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    public List<string> Parameters { get; }
    public List<AstNode> Body { get; }
    public FunctionDeclarationNode(Token identifierToken, string name, List<string> parameters, List<AstNode> body)
    {
        IdentifierToken = identifierToken;
        Name = name;
        Parameters = parameters;
        Body = body;
    }
    public override string ToString() => $"FunctionDeclaration({Name}, [{string.Join(", ", Parameters)}], [\n  {string.Join(",\n  ", Body)}\n])";
}

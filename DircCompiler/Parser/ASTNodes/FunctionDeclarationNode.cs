using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class FunctionDeclarationNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    public Token ReturnTypeToken { get; }
    public List<FunctionParameterNode> Parameters { get; }
    public List<AstNode> Body { get; }
    public FunctionDeclarationNode(Token identifierToken, string name, Token returnTypeToken, List<FunctionParameterNode> parameters, List<AstNode> body)
    {
        IdentifierToken = identifierToken;
        Name = name;
        ReturnTypeToken = returnTypeToken;
        Parameters = parameters;
        Body = body;
    }
    public override string ToString() => $"FunctionDeclaration({ReturnTypeToken.Lexeme} {Name}, [{string.Join(", ", Parameters)}], [\n  {string.Join(",\n  ", Body)}\n])";
}

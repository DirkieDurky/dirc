using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class VariableDeclarationNode : AstNode
{
    public Token TypeToken { get; }
    public Token IdentifierToken { get; }
    public string TypeName => TypeToken.Lexeme;
    public string Name => IdentifierToken.Lexeme;
    public AstNode? Initializer { get; }

    public VariableDeclarationNode(Token typeToken, Token identifierToken, AstNode? initializer = null)
    {
        TypeToken = typeToken;
        IdentifierToken = identifierToken;
        Initializer = initializer;
    }

    public override string ToString() => $"VariableDeclaration({TypeName} {Name}, {Initializer})";
}

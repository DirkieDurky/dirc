using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class VariableDeclarationNode : AstNode
{
    public TypeNode Type { get; }
    public Token IdentifierToken { get; }
    public string TypeName => Type.TypeName;
    public string Name => IdentifierToken.Lexeme;
    public AstNode? Initializer { get; }

    public VariableDeclarationNode(TypeNode type, Token identifierToken, AstNode? initializer = null)
    {
        Type = type;
        IdentifierToken = identifierToken;
        Initializer = initializer;
    }

    public override string ToString() => $"VariableDeclaration({Type} {Name}, {Initializer})";
}

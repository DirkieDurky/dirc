using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class VariableDeclarationNode : AstNode
{
    public TypeNode Type { get; }
    public Token IdentifierToken { get; }
    public string TypeName => Type.Name;
    public string Name => IdentifierToken.Lexeme;
    public AstNode? Initializer { get; }

    public VariableDeclarationNode(TypeNode type, Token identifierToken, AstNode? initializer = null)
    {
        Type = type;
        IdentifierToken = identifierToken;
        Initializer = initializer;
    }

    public override string ToString() => $"VariableDeclaration({Type} {Name}, {Initializer})";

    public override IEnumerable<AstNode> GetChildNodes() =>
        Initializer != null ? [Type, Initializer] : [Type];
}

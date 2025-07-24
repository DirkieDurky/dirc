using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class ArrayDeclarationNode : AstNode
{
    public TypeNode Type { get; }
    public Token IdentifierToken { get; }
    public AstNode Size { get; }
    public AstNode? Initializer { get; }
    public string TypeName => Type.TypeName;
    public string Name => IdentifierToken.Lexeme;

    public ArrayDeclarationNode(TypeNode type, Token identifierToken, AstNode size, AstNode? initializer = null)
    {
        Type = type;
        IdentifierToken = identifierToken;
        Size = size;
        Initializer = initializer;
    }

    public override string ToString() => $"ArrayDeclaration({TypeName} {Name}[{Size}], {Initializer})";
}

using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class ArrayDeclarationNode : AstNode
{
    public Token TypeToken { get; }
    public Token IdentifierToken { get; }
    public AstNode Size { get; }
    public AstNode? Initializer { get; }
    public string TypeName => TypeToken.Lexeme;
    public string Name => IdentifierToken.Lexeme;

    public ArrayDeclarationNode(Token typeToken, Token identifierToken, AstNode size, AstNode? initializer = null)
    {
        TypeToken = typeToken;
        IdentifierToken = identifierToken;
        Size = size;
        Initializer = initializer;
    }

    public override string ToString() => $"ArrayDeclaration({TypeName} {Name}[{Size}], {Initializer})";
}

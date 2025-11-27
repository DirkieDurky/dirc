using System.Drawing;
using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class ArrayDeclarationNode : AstNode
{
    public TypeNode Type { get; }
    public Token IdentifierToken { get; }
    public List<int> Sizes { get; }
    public int TotalSize()
    {
        int product = 1;
        foreach (int? size in Sizes)
        {
            if (size == null) continue;
            product *= (int)size;
        }
        return product;
    }
    public AstNode? Initializer { get; }
    public string TypeName => Type.Name;
    public string Name => IdentifierToken.Lexeme;

    public ArrayDeclarationNode(TypeNode type, Token identifierToken, List<int> sizes, AstNode? initializer = null)
    {
        Type = type;
        IdentifierToken = identifierToken;
        Sizes = sizes;
        Initializer = initializer;
    }

    public override string ToString() => $"ArrayDeclaration({Type} {Name}{string.Join("", Sizes.Select(size => $"[{size}]"))}, {Initializer})";

    public override IEnumerable<AstNode> GetChildNodes() =>
        Initializer != null ? new[] { Type, Initializer } : [Type];
}

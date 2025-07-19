using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class ArrayLiteralNode : AstNode
{
    public List<AstNode> Elements { get; }

    public ArrayLiteralNode(List<AstNode> elements)
    {
        Elements = elements;
    }

    public override string ToString() => $"ArrayLiteral([{string.Join(", ", Elements)}])";
}

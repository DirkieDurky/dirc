using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class StringLiteralNode : AstNode
{
    public Token Str { get; }
    public StringLiteralNode(Token str)
    {
        Str = str;
    }
    public override string ToString() => $"StringLiteral({Str.Literal})";
}

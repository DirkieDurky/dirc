using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class ArrayAccessNode : AstNode
{
    public Token ArrayToken { get; }
    public AstNode Index { get; }
    public string ArrayName => ArrayToken.Lexeme;

    public ArrayAccessNode(Token arrayToken, AstNode index)
    {
        ArrayToken = arrayToken;
        Index = index;
    }

    public override string ToString() => $"ArrayAccess({ArrayName}[{Index}])";
}

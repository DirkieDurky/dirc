using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class ArrayAssignmentNode : AstNode
{
    public Token ArrayToken { get; }
    public AstNode Index { get; }
    public AstNode Value { get; }
    public string ArrayName => ArrayToken.Lexeme;

    public ArrayAssignmentNode(Token arrayToken, AstNode index, AstNode value)
    {
        ArrayToken = arrayToken;
        Index = index;
        Value = value;
    }

    public override string ToString() => $"ArrayAssignment({ArrayName}[{Index}] = {Value})";
}

using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class ArrayAssignmentNode : AstNode
{
    public Token ArrayToken { get; }
    public AstNode Index { get; }
    public AstNode Value { get; }
    public string ArrayName => ArrayToken.Lexeme;
    // The semantic analyzer will fill this in
    public bool ArrayIsPointer { get; set; }

    public ArrayAssignmentNode(Token arrayToken, AstNode index, AstNode value, bool arrayIsPointer = false)
    {
        ArrayToken = arrayToken;
        Index = index;
        Value = value;
        ArrayIsPointer = arrayIsPointer;
    }

    public override string ToString() => $"ArrayAssignment({ArrayName}[{Index}] = {Value})";

    public override IEnumerable<AstNode> GetChildNodes() => [Index, Value];
}

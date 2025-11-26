namespace Dirc.Compiling.Parsing;

public class ArrayAssignmentNode : AstNode
{
    public AstNode Array { get; }
    public AstNode Index { get; }
    public AstNode Value { get; }

    public ArrayAssignmentNode(AstNode array, AstNode index, AstNode value)
    {
        Array = array;
        Index = index;
        Value = value;
    }

    public override string ToString() => $"ArrayAssignment({Array}[{Index}] = {Value})";

    public override IEnumerable<AstNode> GetChildNodes() => [Index, Value];
}

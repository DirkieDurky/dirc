public class NumberLiteralNode : AstNode
{
    public int Value { get; }
    public NumberLiteralNode(int value) => Value = value;
    public override string ToString() => $"Number({Value})";
}

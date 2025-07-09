public class NumberLiteralNode : AstNode, IOperand
{
    public int Value { get; }
    public NumberLiteralNode(int value) => Value = value;
    public override string ToString() => $"Number({Value})";
    public string AsOperand() => Value.ToString();
}

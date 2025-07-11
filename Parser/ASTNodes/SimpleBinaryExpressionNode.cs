public class SimpleBinaryExpressionNode : AstNode, IOperand
{
    public string Operator { get; }
    public NumberLiteralNode Left { get; }
    public NumberLiteralNode Right { get; }
    public SimpleBinaryExpressionNode(string op, NumberLiteralNode left, NumberLiteralNode right)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    public SimpleBinaryExpressionNode(BinaryExpressionNode ben)
    {
        Operator = ben.Operator;
        Left = (NumberLiteralNode)ben.Left;
        Right = (NumberLiteralNode)ben.Right;
    }

    public override string ToString() => $"BinaryExpression({Left}, {Operator}, {Right})";

    public string AsOperand()
    {
        return $"{Left.Value} {Operator} {Right.Value}";
    }
}

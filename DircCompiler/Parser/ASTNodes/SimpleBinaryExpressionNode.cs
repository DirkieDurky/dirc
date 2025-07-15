namespace DircCompiler.Parsing;

public class SimpleBinaryExpressionNode : AstNode, CodeGen.IReturnable
{
    public Operation Operation { get; }
    public NumberLiteralNode Left { get; }
    public NumberLiteralNode Right { get; }
    public SimpleBinaryExpressionNode(Operation operation, NumberLiteralNode left, NumberLiteralNode right)
    {
        Operation = operation;
        Left = left;
        Right = right;
    }

    public SimpleBinaryExpressionNode(BinaryExpressionNode ben)
    {
        Operation = ben.Operation;
        Left = (NumberLiteralNode)ben.Left;
        Right = (NumberLiteralNode)ben.Right;
    }

    public override string ToString() => $"BinaryExpression({Left}, {Operation}, {Right})";

    public string AsOperand()
    {
        return $"{Left.Value} {Operation.GetInlineString()} {Right.Value}";
    }

    public void Free()
    {
        // Doesn't need to be freed
    }
}

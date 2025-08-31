namespace Dirc.Compiling.Parsing;

public class BinaryExpressionNode : AstNode
{
    public Operation Operation { get; }
    public AstNode Left { get; }
    public AstNode Right { get; }
    public BinaryExpressionNode(Operation operation, AstNode left, AstNode right)
    {
        Operation = operation;
        Left = left;
        Right = right;
    }
    public override string ToString() => $"BinaryExpression({Left}, {Operation}, {Right})";
}

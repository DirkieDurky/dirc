namespace Dirc.Parsing;

public class BinaryExpressionNode : AstNode
{
    public string Operator { get; }
    public AstNode Left { get; }
    public AstNode Right { get; }
    public BinaryExpressionNode(string op, AstNode left, AstNode right)
    {
        Operator = op;
        Left = left;
        Right = right;
    }
    public override string ToString() => $"BinaryExpression({Left}, {Operator}, {Right})";
}

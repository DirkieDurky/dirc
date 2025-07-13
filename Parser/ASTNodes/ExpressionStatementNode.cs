namespace Dirc.Parsing;

public class ExpressionStatementNode : AstNode
{
    public AstNode Expression { get; }
    public ExpressionStatementNode(AstNode expr) => Expression = expr;
    public override string ToString() => $"ExpressionStatement({Expression})";
}

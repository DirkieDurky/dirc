namespace Dirc.Compiling.Parsing;

public class ExpressionStatementNode : AstNode
{
    public AstNode Expression { get; }
    public ExpressionStatementNode(AstNode expr) => Expression = expr;
    public override string ToString() => $"ExpressionStatement({Expression})";

    public override IEnumerable<AstNode> GetChildNodes() => [Expression];
}

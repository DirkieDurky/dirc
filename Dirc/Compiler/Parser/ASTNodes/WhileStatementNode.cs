namespace Dirc.Compiling.Parsing;

public class WhileStatementNode : AstNode
{
    public AstNode Condition { get; }
    public List<AstNode> Body { get; }

    public WhileStatementNode(AstNode condition, List<AstNode> body)
    {
        Condition = condition;
        Body = body;
    }

    public override string ToString() => $"While({Condition}, [\n  {string.Join(",\n  ", Body)}\n])";
}

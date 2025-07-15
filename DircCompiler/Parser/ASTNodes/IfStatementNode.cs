namespace DircCompiler.Parsing;

public class IfStatementNode : AstNode
{
    public ConditionNode Condition { get; }
    public List<AstNode> Body { get; }

    public IfStatementNode(ConditionNode condition, List<AstNode> body)
    {
        Condition = condition;
        Body = body;
    }

    public override string ToString() => $"If({Condition}, [\n  {string.Join(",\n  ", Body)}\n])";
}

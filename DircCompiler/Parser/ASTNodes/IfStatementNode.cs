namespace DircCompiler.Parsing;

public class IfStatementNode : AstNode
{
    public ConditionNode Condition { get; }
    public List<AstNode> Body { get; }
    public List<AstNode>? ElseBody { get; }

    public IfStatementNode(ConditionNode condition, List<AstNode> body, List<AstNode>? elseBody)
    {
        Condition = condition;
        Body = body;
        ElseBody = elseBody;
    }

    public override string ToString() => $"If({Condition}, [\n  {string.Join(",\n  ", Body)}\n])";
}

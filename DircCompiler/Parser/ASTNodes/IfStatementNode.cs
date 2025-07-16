namespace DircCompiler.Parsing;

public class IfStatementNode : AstNode
{
    public AstNode Condition { get; }
    public List<AstNode> Body { get; }
    public List<AstNode>? ElseBody { get; }

    public IfStatementNode(AstNode condition, List<AstNode> body, List<AstNode>? elseBody)
    {
        Condition = condition;
        Body = body;
        ElseBody = elseBody;
    }

    public override string ToString()
    {
        string result = $"If({Condition}, [\n  {string.Join(",\n  ", Body)}\n])";
        if (ElseBody != null)
        {
            result += $", Else, [\n  {string.Join(",\n  ", ElseBody)}\n]";
        }
        return result;
    }
}

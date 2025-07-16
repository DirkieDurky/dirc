namespace DircCompiler.Parsing;

public class WhileNode : AstNode
{
    public AstNode Condition { get; }
    public List<AstNode> Body { get; }

    public WhileNode(AstNode condition, List<AstNode> body)
    {
        Condition = condition;
        Body = body;
    }

    public override string ToString() => $"If({Condition}, [\n  {string.Join(",\n  ", Body)}\n])";
}

namespace Dirc.Compiling.Parsing;

public class ForStatementNode : AstNode
{
    public AstNode? Initialization { get; }
    public AstNode? Condition { get; }
    public AstNode? Increment { get; }
    public List<AstNode> Body { get; }

    public ForStatementNode(AstNode? initialization, AstNode? condition, AstNode? increment, List<AstNode> body)
    {
        Initialization = initialization;
        Condition = condition;
        Increment = increment;
        Body = body;
    }

    public override string ToString() =>
        $"ForStatement(init: {Initialization}, cond: {Condition}, incr: {Increment}, body: [{string.Join(", ", Body)}])";

    public override IEnumerable<AstNode> GetChildNodes()
    {
        if (Initialization != null) yield return Initialization;
        if (Condition != null) yield return Condition;
        if (Increment != null) yield return Increment;
        foreach (var node in Body) yield return node;
    }
}

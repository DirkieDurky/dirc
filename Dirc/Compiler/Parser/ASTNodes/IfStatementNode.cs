namespace Dirc.Compiling.Parsing;

public class IfStatementNode : AstNode
{
    public AstNode Condition { get; }
    public List<AstNode> Body { get; }
    public List<AstNode>? ElseBody { get; }
    public IfStatementNode(AstNode condition, List<AstNode> body, List<AstNode>? elseBody = null)
    {
        Condition = condition;
        Body = body;
        ElseBody = elseBody;
    }

    public override string ToString() => $"IfStatement({Condition}, [{string.Join(", ", Body)}]" + (ElseBody != null ? $", [{string.Join(", ", ElseBody)}])" : ")");

    public override IEnumerable<AstNode> GetChildNodes() => ElseBody == null ? Body.Append(Condition) : Body.Concat(ElseBody).Append(Condition);
}

public class CallExpressionNode : AstNode
{
    public string Callee { get; }
    public List<AstNode> Arguments { get; }
    public CallExpressionNode(string callee, List<AstNode> arguments)
    {
        Callee = callee;
        Arguments = arguments;
    }
    public override string ToString() => $"Call({Callee}, [{string.Join(", ", Arguments)}])";
}

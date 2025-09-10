using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class CallExpressionNode : AstNode
{
    public Token CalleeToken { get; }
    public string Callee { get; }
    public List<AstNode> Arguments { get; }
    public CallExpressionNode(Token calleeToken, string callee, List<AstNode> arguments)
    {
        CalleeToken = calleeToken;
        Callee = callee;
        Arguments = arguments;
    }
    public override string ToString() => $"Call({Callee}, [{string.Join(", ", Arguments)}])";

    public override IEnumerable<AstNode> GetChildNodes() => Arguments;
}

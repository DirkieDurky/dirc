using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class ReturnStatementNode : AstNode
{
    public AstNode? ReturnValue { get; }
    public ReturnStatementNode(AstNode returnValue)
    {
        ReturnValue = returnValue;
    }
    public override string ToString() => $"Return({ReturnValue})";

    public override IEnumerable<AstNode> GetChildNodes() => ReturnValue != null ? [ReturnValue] : [];
}

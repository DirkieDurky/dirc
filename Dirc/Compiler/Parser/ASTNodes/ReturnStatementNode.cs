namespace Dirc.Compiling.Parsing;

public class ReturnStatementNode : AstNode
{
    public AstNode ReturnValue;
    public ReturnStatementNode(AstNode returnValue)
    {
        ReturnValue = returnValue;
    }
    public override string ToString() => $"Return({ReturnValue})";
}

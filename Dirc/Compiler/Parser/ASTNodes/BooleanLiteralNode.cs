namespace Dirc.Compiling.Parsing;

public class BooleanLiteralNode : AstNode, CodeGen.IReturnable
{
    public bool Value { get; }

    public BooleanLiteralNode(bool value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value ? "true" : "false";
    }

    public void Free()
    {
    }

    public string AsOperand()
    {
        return Value ? "1" : "0";
    }

    public override IEnumerable<AstNode> GetChildNodes() => [];
}

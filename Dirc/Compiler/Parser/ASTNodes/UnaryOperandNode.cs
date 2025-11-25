namespace Dirc.Compiling.Parsing;

public class UnaryOperationNode : AstNode
{
    public UnaryOperationType OperationType { get; }
    public AstNode Operand { get; }
    public UnaryOperationNode(UnaryOperationType operationType, AstNode operand)
    {
        OperationType = operationType;
        Operand = operand;
    }
    public override string ToString() => $"UnaryOperation({OperationType}, {Operand})";

    public override IEnumerable<AstNode> GetChildNodes() => [Operand];
}

public enum UnaryOperationType
{
    Negate,
}

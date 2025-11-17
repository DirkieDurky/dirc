namespace Dirc.Compiling.Parsing;

public class ArrLenNode : AstNode
{
    public IdentifierNode Array { get; }
    public int Dimension { get; }
    public int ComputedLength { get; set; }

    public ArrLenNode(IdentifierNode array, int dimension)
    {
        Array = array;
        Dimension = dimension;
    }

    public override string ToString()
    {
        return $"ArrLenNode({Array}, {Dimension})";
    }
    public override IEnumerable<AstNode> GetChildNodes() => [];
}

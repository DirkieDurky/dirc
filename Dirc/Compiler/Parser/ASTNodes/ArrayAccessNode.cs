using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class ArrayAccessNode : AstNode
{
    public AstNode Array { get; }
    public AstNode Index { get; }

    public ArrayAccessNode(AstNode array, AstNode index)
    {
        Array = array;
        Index = index;
    }

    public override string ToString() => $"ArrayAccess({Array}[{Index}])";
    public override IEnumerable<AstNode> GetChildNodes() => [Index];
}

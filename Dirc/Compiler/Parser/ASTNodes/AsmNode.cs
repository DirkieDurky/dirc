namespace Dirc.Compiling.Parsing;

public class AsmNode(string code) : AstNode
{
    public string Code = code;

    public override string ToString()
    {
        return $"AsmNode({Code})";
    }
    public override IEnumerable<AstNode> GetChildNodes() => [];
}

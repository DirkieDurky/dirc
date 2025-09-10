namespace Dirc.Compiling.Parsing;

public abstract class AstNode
{
    public abstract override string ToString();
    public virtual IEnumerable<AstNode> GetChildNodes() => Enumerable.Empty<AstNode>();
}

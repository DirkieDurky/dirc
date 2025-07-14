namespace Dirc.Parsing;

public class ProgramNode : AstNode
{
    public List<AstNode> Statements { get; }
    public ProgramNode(List<AstNode> statements) => Statements = statements;
    public override string ToString() => $"Program([\n  {string.Join(",\n  ", Statements)}\n])";
}

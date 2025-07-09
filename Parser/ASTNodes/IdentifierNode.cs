public class IdentifierNode : AstNode, IOperand
{
    public string Name { get; }
    public IdentifierNode(string name) => Name = name;
    public override string ToString() => $"Identifier({Name})";
    public string AsOperand() => Name;
}

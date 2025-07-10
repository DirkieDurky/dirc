public class IdentifierNode : AstNode
{
    public string Name { get; }
    public IdentifierNode(string name) => Name = name;
    public override string ToString() => $"Identifier({Name})";
}

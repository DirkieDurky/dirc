namespace Dirc.Parsing;

public class VariableAssignmentNode : AstNode
{
    public string Name { get; }
    public AstNode? Initializer { get; }

    public VariableAssignmentNode(string name, AstNode? initializer = null)
    {
        Name = name;
        Initializer = initializer;
    }

    public override string ToString() => $"VariableDeclaration({Name}, {Initializer})";
}

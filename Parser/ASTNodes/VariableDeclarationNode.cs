namespace Dirc.Parsing;

public class VariableDeclarationNode : AstNode
{
    public string Name { get; }
    public AstNode? Initializer { get; }

    public VariableDeclarationNode(string name, AstNode? initializer = null)
    {
        Name = name;
        Initializer = initializer;
    }

    public override string ToString() => $"VariableDeclaration({Name}, {Initializer})";
}

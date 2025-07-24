using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class AddressOfNode : AstNode
{
    public IdentifierNode Variable { get; }
    public AddressOfNode(IdentifierNode variable)
    {
        Variable = variable;
    }
    public override string ToString() => $"AddressOf({Variable})";
}

using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class AddressOfNode : AstNode
{
    public IdentifierNode Variable { get; }
    public AddressOfNode(IdentifierNode variable)
    {
        Variable = variable;
    }
    public override string ToString() => $"AddressOf({Variable})";
}

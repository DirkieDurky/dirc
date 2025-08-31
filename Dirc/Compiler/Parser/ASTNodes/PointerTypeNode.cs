using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class PointerTypeNode : TypeNode
{
    public TypeNode BaseType { get; }

    public PointerTypeNode(Token identifierToken, TypeNode baseType) : base(identifierToken, baseType.TypeName + "*")
    {
        BaseType = baseType;
    }

    public override string ToString() => $"PointerType({TypeName})";
}

using System.Dynamic;
using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public abstract class TypeNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    public bool IsArray { get; }
    public List<int> ArraySizes { get; }
    public TypeNode(Token identifierToken, string name, bool isArray, List<int> arraySizes)
    {
        IdentifierToken = identifierToken;
        Name = name;
        IsArray = isArray;
        ArraySizes = arraySizes;
    }
    public override string ToString() => $"Type({Name})";
    public override IEnumerable<AstNode> GetChildNodes() => [];
}

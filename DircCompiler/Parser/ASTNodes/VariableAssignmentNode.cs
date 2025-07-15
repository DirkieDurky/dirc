using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class VariableAssignmentNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    // True if the "var" keyword was used.
    // If this is true we know for sure this should be a declaration.
    // If false it's only a declaration if the variable hasn't been declared before.
    public bool IsDeclaration { get; }
    public AstNode? Initializer { get; }

    public VariableAssignmentNode(Token identifierToken, string name, bool isDeclaration, AstNode? initializer = null)
    {
        IdentifierToken = identifierToken;
        Name = name;
        IsDeclaration = isDeclaration;
        Initializer = initializer;
    }

    public override string ToString() => $"VariableDeclaration({Name}, {Initializer})";
}

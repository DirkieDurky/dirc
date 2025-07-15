using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class VariableAssignmentNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    // True = We're sure it's an declaration
    // False = It could be either assignment or declaration
    public bool IsDeclaration { get; }
    // True = We're sure it's an assignment
    // False = It could be either assignment or declaration
    public bool IsAssignment { get; }
    public AstNode? Value { get; }

    public VariableAssignmentNode(Token identifierToken, string name, bool isDeclaration, bool isAssignment, AstNode? initializer = null)
    {
        IdentifierToken = identifierToken;
        Name = name;
        IsDeclaration = isDeclaration;
        IsAssignment = isAssignment;
        Value = initializer;
    }

    public override string ToString() => $"VariableDeclaration({Name}, {Value})";
}

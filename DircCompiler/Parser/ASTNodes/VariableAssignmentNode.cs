using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class VariableAssignmentNode : AstNode
{
    public Token IdentifierToken { get; }
    public string Name { get; }
    public AstNode Value { get; }

    public VariableAssignmentNode(Token identifierToken, string name, AstNode value)
    {
        IdentifierToken = identifierToken;
        Name = name;
        Value = value;
    }

    public override string ToString() => $"VariableAssignment({Name}, {Value})";
}

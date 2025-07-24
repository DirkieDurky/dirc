using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class VariableAssignmentNode : AstNode
{
    public AstNode Target { get; }
    public Token TargetName { get; }
    public string Name => TargetName.Lexeme;
    public AstNode Value { get; }

    public VariableAssignmentNode(AstNode target, Token targetName, AstNode value)
    {
        Target = target;
        TargetName = targetName;
        Value = value;
    }

    public override string ToString() => $"VariableAssignment({Target}, {Value})";
}

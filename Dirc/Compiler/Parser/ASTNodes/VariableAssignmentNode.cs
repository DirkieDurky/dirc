using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

public class VariableAssignmentNode : AstNode
{
    public AstNode Target { get; }
    public Token? TargetName { get; }
    public string? Name => TargetName == null ? null : TargetName.Lexeme;
    public AstNode Value { get; }

    public VariableAssignmentNode(AstNode target, Token? targetName, AstNode value)
    {
        Target = target;
        TargetName = targetName;
        Value = value;
    }

    public override string ToString() => $"VariableAssignment({Target}, {Value})";

    public override IEnumerable<AstNode> GetChildNodes() => [Target, Value];
}

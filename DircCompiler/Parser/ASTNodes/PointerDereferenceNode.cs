using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class PointerDereferenceNode : AstNode
{
    public AstNode PointerExpression { get; }
    public PointerDereferenceNode(AstNode pointerExpression)
    {
        PointerExpression = pointerExpression;
    }
    public override string ToString() => $"PointerDereference({PointerExpression})";
}

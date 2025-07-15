using DircCompiler.CodeGen;

namespace DircCompiler.Parsing;

public class ConditionNode : AstNode
{
    public Comparer Comparer { get; }
    public AstNode Left { get; }
    public AstNode Right { get; }

    public ConditionNode(Comparer comparer, AstNode left, AstNode right)
    {
        Comparer = comparer;
        Left = left;
        Right = right;
    }

    public override string ToString() => $"Condition({Left}, {Comparer}, {Right})";
}

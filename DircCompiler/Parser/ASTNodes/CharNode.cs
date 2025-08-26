namespace DircCompiler.Parsing;

public class CharNode : AstNode
{
    public char Value { get; } // The character

    public CharNode(char value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"CharNode({Value})";
    }
}

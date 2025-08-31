namespace Dirc.Compiling.Parsing;

public class NumberLiteralNode : AstNode, CodeGen.IReturnable
{
    public string Value { get; } // The number
    public NumberLiteralType Type { get; }

    public NumberLiteralNode(NumberLiteralType type, string value)
    {
        Type = type;
        Value = value;
    }

    public NumberLiteralNode(int value)
    {
        Type = NumberLiteralType.Decimal;
        Value = value.ToString();
    }

    public override string ToString() => $"Number({Value})";
    public string AsOperand() => Value.ToString();

    public void Free()
    {
        // Doesn't need to be freed
    }
}

public enum NumberLiteralType
{
    Decimal,
    Binary,
    Hexadecimal,
}

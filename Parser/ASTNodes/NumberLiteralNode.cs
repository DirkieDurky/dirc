public class NumberLiteralNode : AstNode, IOperand
{
    public string Value { get; }
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
}

public enum NumberLiteralType
{
    Decimal,
    Binary,
    Hexadecimal,
}

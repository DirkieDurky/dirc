class Identifier
{
    // True if it's an immediate value, false if it's a reference to a register
    public IdentifierType Type;
    public string Value;

    public Identifier(IdentifierType type, string identifier)
    {
        Type = type;
        Value = identifier;
    }

    public enum IdentifierType
    {
        Immediate,
        Register,
        RamPointer,
    }
}

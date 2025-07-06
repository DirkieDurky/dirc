class Identifier
{
    // True if it's an immediate value, false if it's a reference to a register
    public bool Immediate;
    public string Value;

    public Identifier(bool immediate, string identifier)
    {
        Immediate = immediate;
        Value = identifier;
    }
}

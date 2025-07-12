class ReturnOperand
{
    // Return value
    public IOperand Value { get; }
    // Registers to free when value is freed
    private List<Register> _toFree = new();
    private CodeGenContext _context;

    public ReturnOperand(CodeGenContext context, IOperand value, List<IOperand> toFree)
    {
        _context = context;
        Value = value;
        _toFree = toFree.OfType<Register>().ToList();
    }

    public void Free()
    {
        foreach (Register reg in _toFree)
        {
            _context.Allocator.Free(reg);
        }
    }
}

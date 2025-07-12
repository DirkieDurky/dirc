class CodeGenContext : ICloneable
{
    public Allocator Allocator { get; }
    public ExpressionCodeFactory ExprFactory { get; }
    public FunctionCodeFactory FuncFactory { get; }
    public CodeGenerator CodeGen { get; }
    public Dictionary<string, Register> SymbolTable { get; }
    public Dictionary<string, Variable> VariableTable { get; set; } = new();
    public FunctionTable FunctionTable { get; } = new();
    public int NextVariableOffset { get; set; } = 0;
    public Register? FramePointer { get; set; } = null;

    public CodeGenContext(Allocator allocator, ExpressionCodeFactory exprFactory, FunctionCodeFactory funcFactory, Dictionary<string, Register> symbolTable, CodeGenerator codeGen)
    {
        Allocator = allocator;
        ExprFactory = exprFactory;
        FuncFactory = funcFactory;
        SymbolTable = symbolTable;
        CodeGen = codeGen;
    }

    public object Clone()
    {
        var newContext = new CodeGenContext(Allocator, ExprFactory, FuncFactory, SymbolTable.ToDictionary(x => x.Key, x => x.Value), CodeGen);
        newContext.VariableTable = VariableTable.ToDictionary(x => x.Key, x => x.Value);
        newContext.NextVariableOffset = NextVariableOffset;
        newContext.FramePointer = FramePointer;
        return newContext;
    }
}

class CodeGenContext : ICloneable
{
    public ExpressionCodeFactory ExprFactory { get; }
    public FunctionCodeFactory FuncFactory { get; }
    public CodeGenerator CodeGen { get; }
    public Dictionary<string, RegisterEnum> SymbolTable { get; }
    public FunctionTable FunctionTable { get; } = new();

    public CodeGenContext(ExpressionCodeFactory exprFactory, FunctionCodeFactory funcFactory, Dictionary<string, RegisterEnum> symbolTable, CodeGenerator codeGen)
    {
        ExprFactory = exprFactory;
        FuncFactory = funcFactory;
        SymbolTable = symbolTable;
        CodeGen = codeGen;
    }

    public object Clone()
    {
        return new CodeGenContext(ExprFactory, FuncFactory, SymbolTable.ToDictionary(x => x.Key, x => x.Value), CodeGen);
    }
}

class CodeGenContext
{
    public Allocator Allocator { get; }
    public ExpressionCodeFactory ExprFactory { get; }
    public FunctionCodeFactory FuncFactory { get; }

    public CodeGenContext(Allocator allocator, ExpressionCodeFactory exprFactory, FunctionCodeFactory funcFactory)
    {
        Allocator = allocator;
        ExprFactory = exprFactory;
        FuncFactory = funcFactory;
    }
}

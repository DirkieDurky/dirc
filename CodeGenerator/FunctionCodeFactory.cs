class FunctionCodeFactory
{
    private readonly Allocator _allocator;
    private readonly ExpressionCodeFactory _exprFactory;

    public FunctionCodeFactory(Allocator allocator, ExpressionCodeFactory exprFactory)
    {
        _allocator = allocator;
        _exprFactory = exprFactory;
    }

    public List<string> Generate(FunctionDeclarationNode node)
    {
        List<string> code =
        [
            $"label {node.Name}:",
            $"push lr _ _",
        ];
        foreach (var stmt in node.Body)
        {
            code.AddRange(_exprFactory.Generate(stmt));
        }
        code.Add("pop _ _ lr");
        code.Add("return _ _ _");
        code.Add("");
        return code;
    }
}

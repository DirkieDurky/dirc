class ExpressionCodeFactory
{
    private readonly Allocator _allocator;

    public ExpressionCodeFactory(Allocator allocator)
    {
        _allocator = allocator;
    }

    public List<string> Generate(AstNode node)
    {
        switch (node)
        {
            case ExpressionStatementNode exprStmt:
                return Generate(exprStmt.Expression);
            case CallExpressionNode call:
                return GenerateCall(call);
            case BinaryExpressionNode bin:
                return GenerateBinary(bin);
            case NumberLiteralNode num:
                return [$"mov|i1 {num.Value} _ r0"];
            case IdentifierNode id:
                return [$"// use identifier {id.Name}"];
            default:
                throw new Exception("Unhandled node");
        }
    }

    private List<string> GenerateCall(CallExpressionNode node)
    {
        List<string> code = [];
        // Evaluate arguments and move to registers r0, r1, ...
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            var argCode = Generate(node.Arguments[i]);
            code.AddRange(argCode);
            code.Add($"mov r0 _ r{i}"); // Move result to correct param register
        }
        code.Add($"call {node.Callee} _ _");
        return code;
    }

    private List<string> GenerateBinary(BinaryExpressionNode node)
    {
        var code = new List<string>();
        var leftCode = Generate(node.Left);
        code.AddRange(leftCode);
        code.Add("push r0 _ _");
        var rightCode = Generate(node.Right);
        code.AddRange(rightCode);
        code.Add("pop _ _ r1");
        string op = node.Operator switch
        {
            "+" => "add",
            "-" => "sub",
            "*" => "mul",
            "/" => "div",
            "|" => "or",
            "&" => "and",
            "^" => "xor",
            _ => node.Operator
        };
        code.Add($"{op} r1 r0 r0");
        return code;
    }
}

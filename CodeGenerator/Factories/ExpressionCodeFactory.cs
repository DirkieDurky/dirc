class ExpressionCodeFactory
{
    public void Generate(AstNode node, CodeGenContext context)
    {
        switch (node)
        {
            case ExpressionStatementNode exprStmt:
                Generate(exprStmt.Expression, context);
                break;
            case CallExpressionNode call:
                GenerateCall(call, context);
                break;
            case BinaryExpressionNode bin:
                GenerateBinary(bin, context);
                break;
            case IdentifierNode id:
                if (context.SymbolTable.TryGetValue(id.Name, out var reg))
                    context.CodeGen.EmitMov(new Register(reg), RegisterEnum.r0);
                else
                    throw new Exception($"Identifier {id.Name} does not exist.");
                break;
            default:
                throw new Exception("Unhandled node");
        }
    }

    private void GenerateCall(CallExpressionNode node, CodeGenContext context)
    {
        // Save in use caller saved registers
        List<RegisterEnum> toSave = Allocator.CallerSaved.Where(Allocator.InUse.Contains).ToList();
        foreach (var reg in toSave)
        {
            context.CodeGen.EmitPush(new Register(reg));
        }

        // Put arguments in the right locations. Spill arguments to stack if there are more arguments than argumentRegisters.
        for (int i = 0; i < node.Arguments.Count; i++)
        {

            context.CodeGen.EmitMov(new Register(RegisterEnum.r0), (RegisterEnum)i);
        }

        context.CodeGen.EmitFunctionCall(node.Callee);

        // Restore saved registers
        toSave.Reverse();
        foreach (var reg in toSave)
        {
            context.CodeGen.EmitPop(reg);
        }
    }

    private void GenerateBinary(BinaryExpressionNode node, CodeGenContext context)
    {
        Generate(node.Left, context);
        context.CodeGen.EmitPush(new Register(RegisterEnum.r0));
        Generate(node.Right, context);
        context.CodeGen.EmitPop(RegisterEnum.r1);
        var op = node.Operator switch
        {
            "+" => Operation.Add,
            "-" => Operation.Sub,
            "*" => Operation.Mul,
            "/" => Operation.Div,
            "|" => Operation.Or,
            "&" => Operation.And,
            "^" => Operation.Xor,
            _ => throw new Exception($"Unknown operator {node.Operator}")
        };
        context.CodeGen.EmitBinaryOperation(op, new Register(RegisterEnum.r1), new Register(RegisterEnum.r0), RegisterEnum.r0);
    }
}

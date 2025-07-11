class ExpressionCodeFactory
{
    public IOperand? Generate(AstNode node, CodeGenContext context)
    {
        switch (node)
        {
            case ExpressionStatementNode exprStmt:
                return Generate(exprStmt.Expression, context);
            case CallExpressionNode call:
                return GenerateCall(call, context);
            case BinaryExpressionNode bin:
                return GenerateBinary(bin, context);
            case IdentifierNode id:
                if (context.SymbolTable.TryGetValue(id.Name, out var reg)) return new Register(reg);
                else throw new Exception($"Identifier {id.Name} does not exist.");
            case NumberLiteralNode number:
                return number;
            default:
                throw new Exception($"Unhandled node {node}");
        }
    }

    private IOperand? GenerateCall(CallExpressionNode node, CodeGenContext context)
    {
        Function callee;
        try
        {
            callee = context.FunctionTable.Lookup(node.Callee);
        }
        catch
        {
            throw new Exception($"Call made to unknown function '{node.Callee}'");
        }

        if (callee.Parameters.Count() != node.Arguments.Count) throw new Exception($"Function {callee} takes {callee.Parameters.Count()} arguments. {node.Arguments.Count} given.");

        // Save in use caller saved registers
        List<RegisterEnum> toSave = Allocator.CallerSaved.Where(Allocator.InUse.Contains).ToList();
        foreach (var reg in toSave)
        {
            context.CodeGen.EmitPush(new Register(reg));
        }

        // Put arguments in the right locations. Spill arguments to stack if there are more arguments than argumentRegisters.
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            IOperand argument = Generate(node.Arguments[i], context) ?? throw new Exception("Argument was not set");
            context.CodeGen.EmitMov(argument, (RegisterEnum)i);
            if (argument is Register reg) Allocator.Free(reg.RegisterEnum);
        }

        context.CodeGen.EmitFunctionCall(node.Callee);

        // Restore saved registers
        toSave.Reverse();
        foreach (var reg in toSave)
        {
            context.CodeGen.EmitPop(reg);
        }

        return null;
    }

    private IOperand? GenerateBinary(BinaryExpressionNode node, CodeGenContext context)
    {
        IOperand leftOperand = Generate(node.Left, context) ?? throw new Exception("Left operand of binary expression is missing");
        IOperand rightOperand = Generate(node.Right, context) ?? throw new Exception("Right operand of binary expression is missing");
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
        RegisterEnum result = Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(op, leftOperand, rightOperand, result);
        return new Register(result);
    }
}

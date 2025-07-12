class ExpressionCodeFactory
{
    public IOperand? Generate(AstNode node, CodeGenContext context)
    {
        switch (node)
        {
            case ExpressionStatementNode exprStmt:
                return Generate(exprStmt.Expression, context);
            case VariableDeclarationNode varDecl:
                return GenerateVariableDeclaration(varDecl, context);
            case CallExpressionNode call:
                return GenerateCall(call, context);
            case BinaryExpressionNode bin:
                return GenerateBinary(bin, context);
            case IdentifierNode id:
                return GenerateIdentifier(id, context);
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
        List<Register> toSave = Allocator.CallerSaved.Where(context.Allocator.InUse.Contains).ToList();
        foreach (Register reg in toSave)
        {
            context.CodeGen.EmitPush(reg);
        }

        for (int i = 0; i < node.Arguments.Count; i++)
        {
            IOperand argument = Generate(node.Arguments[i], context) ?? throw new Exception("Argument was not set");
            context.CodeGen.EmitMov(argument, Allocator.ArgumentRegisters.ElementAt(i));
            context.Allocator.Free(argument);
        }

        context.CodeGen.EmitFunctionCall(node.Callee);

        // Restore saved registers
        toSave.Reverse();
        foreach (Register reg in toSave)
        {
            context.CodeGen.EmitPop(reg);
        }

        return null;
    }

    private IOperand? GenerateBinary(BinaryExpressionNode node, CodeGenContext context)
    {
        IOperand leftOperand = Generate(node.Left, context) ?? throw new Exception("Left operand of binary expression is missing");
        IOperand rightOperand = Generate(node.Right, context) ?? throw new Exception("Right operand of binary expression is missing");

        if (leftOperand is NumberLiteralNode && rightOperand is NumberLiteralNode)
        {
            return new SimpleBinaryExpressionNode(node);
        }

        Operation op = node.Operator switch
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
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(op, leftOperand, rightOperand, result);
        context.Allocator.Free(leftOperand);
        context.Allocator.Free(rightOperand);
        return result;
    }

    private IOperand? GenerateVariableDeclaration(VariableDeclarationNode node, CodeGenContext context)
    {
        int offset = context.AllocateVariable(node.Name);

        if (node.Initializer != null)
        {
            IOperand initialValue = Generate(node.Initializer, context) ?? throw new Exception("Initializer expression failed to generate");

            Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);

            context.CodeGen.EmitBinaryOperation(Operation.Sub, Register.FP, new NumberLiteralNode(NumberLiteralType.Decimal, offset.ToString()), tmp);
            context.CodeGen.EmitStore(initialValue, tmp);

            context.Allocator.Free(initialValue);
            context.Allocator.Free(tmp);
        }

        return null;
    }

    private IOperand GenerateIdentifier(IdentifierNode node, CodeGenContext context)
    {
        if (context.SymbolTable.TryGetValue(node.Name, out Register? reg))
        {
            return reg;
        }

        if (context.VariableTable.TryGetValue(node.Name, out Variable? variable))
        {
            Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);

            Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            context.CodeGen.EmitBinaryOperation(Operation.Sub, Register.FP, new NumberLiteralNode(variable.FramePointerOffset * CodeGenContext.StackAlignment), tmp);
            context.CodeGen.EmitLoad(tmp, result);

            context.Allocator.Free(tmp);

            return result;
        }

        throw new Exception($"Undefined identifier '{node.Name}' was used");
    }
}

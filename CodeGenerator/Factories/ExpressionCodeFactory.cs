using Dirc.CodeGen.Allocating;
using Dirc.Parsing;

namespace Dirc.CodeGen;

class ExpressionCodeFactory
{
    public IReturnable? Generate(AstNode node, CodeGenContext context)
    {
        switch (node)
        {
            case ExpressionStatementNode exprStmt:
                return Generate(exprStmt.Expression, context);
            case VariableAssignmentNode varDecl:
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

    private IReturnable? GenerateCall(CallExpressionNode node, CodeGenContext context)
    {
        context.CompilerContext.PushCall(context.CompilerContext.CurrentFilePath, node.CalleeToken.Line);
        Function callee;
        try
        {
            callee = context.FunctionTable.Lookup(node.Callee);
        }
        catch
        {
            throw new CodeGenException($"Call made to unknown function '{node.Callee}'",
                node.CalleeToken,
                context.CompilerContext
            );
        }

        if (callee.Parameters.Count() != node.Arguments.Count)
        {
            throw new CodeGenException($"Function '{callee}' takes {callee.Parameters.Count()} arguments. {node.Arguments.Count} given.",
                node.CalleeToken,
                context.CompilerContext
            );
        }

        // Save in use caller saved registers
        List<Register> toSave = context.Allocator.TrackedCallerSavedRegisters.Where(x => x.InUse).ToList();
        foreach (Register reg in toSave)
        {
            context.CodeGen.EmitPush(new ReadonlyRegister(reg));
        }

        List<Register> registersToFree = new();
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            IReturnable argument = Generate(node.Arguments[i], context) ?? throw new Exception("Argument was not set");
            RegisterEnum argumentSlotEnum = Allocator.ArgumentRegisters.ElementAt(i);
            if (argument is ReturnRegister reg && reg.RegisterEnum != argumentSlotEnum)
            {
                Register argumentSlot = context.Allocator.Use(argumentSlotEnum);
                registersToFree.Add(argumentSlot);
                context.CodeGen.EmitMov(argument, argumentSlot);
            }
            argument.Free();
        }

        context.CodeGen.EmitFunctionCall(node.Callee);

        foreach (Register reg in registersToFree)
        {
            reg.Free();
        }

        // Restore saved registers
        toSave.Reverse();
        foreach (Register reg in toSave)
        {
            context.CodeGen.EmitPop(reg);
        }

        context.CompilerContext.PopCall();

        return null;
    }

    private IReturnable? GenerateBinary(BinaryExpressionNode node, CodeGenContext context)
    {
        IReturnable leftOperand = Generate(node.Left, context) ?? throw new Exception("Left operand of binary expression is missing");
        IReturnable rightOperand = Generate(node.Right, context) ?? throw new Exception("Right operand of binary expression is missing");

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
        leftOperand.Free();
        rightOperand.Free();

        return new ReturnRegister(result);
    }

    private IReturnable? GenerateVariableDeclaration(VariableAssignmentNode node, CodeGenContext context)
    {
        int offset = context.AllocateVariable(node.Name);

        if (node.Initializer != null)
        {
            IReturnable initialValue = Generate(node.Initializer, context) ?? throw new Exception("Initializer expression failed to generate");

            Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);

            context.CodeGen.EmitBinaryOperation(
                Operation.Sub,
                ReadonlyRegister.FP,
                new NumberLiteralNode(NumberLiteralType.Decimal, offset.ToString()),
                tmp
            );
            context.CodeGen.EmitStore(initialValue, new ReadonlyRegister(tmp));

            tmp.Free();
            initialValue.Free();
        }

        return null;
    }

    private IReturnable GenerateIdentifier(IdentifierNode node, CodeGenContext context)
    {
        if (context.SymbolTable.TryGetValue(node.Name, out Register? reg))
        {
            return new ReturnRegister(reg);
        }

        if (context.VariableTable.TryGetValue(node.Name, out Variable? variable))
        {
            Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            context.CodeGen.EmitBinaryOperation(
                Operation.Sub,
                ReadonlyRegister.FP,
                new NumberLiteralNode(variable.FramePointerOffset * CodeGenContext.StackAlignment),
                tmp
            );

            Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            context.CodeGen.EmitLoad(new ReadonlyRegister(tmp), result);
            tmp.Free();

            return new ReturnRegister(result);
        }

        throw new CodeGenException($"Undefined identifier '{node.Name}' was used", node.IdentifierToken, context.CompilerContext);
    }
}

using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class ExpressionCodeFactory
{
    private readonly CompilerOptions _compilerOptions;
    private readonly LabelGenerator _labelGenerator;

    public ExpressionCodeFactory(CompilerOptions compilerOptions, LabelGenerator labelGenerator)
    {
        _compilerOptions = compilerOptions;
        _labelGenerator = labelGenerator;
    }

    public IReturnable? Generate(AstNode node, CodeGenContext context)
    {
        switch (node)
        {
            case ExpressionStatementNode exprStmt:
                return Generate(exprStmt.Expression, context);
            case VariableDeclarationNode varDeclarationNode:
                return GenerateVariableDeclaration(varDeclarationNode, context);
            case VariableAssignmentNode varAssignmentNode:
                return GenerateVariableAssignment(varAssignmentNode, context);
            case CallExpressionNode call:
                return GenerateCall(call, context);
            case BinaryExpressionNode bin:
                return GenerateBinary(bin, context);
            case IdentifierNode id:
                return GenerateIdentifier(id, context);
            case BooleanLiteralNode boolean:
                return boolean;
            case NumberLiteralNode number:
                return number;
            case ConditionNode condition:
                return GenerateCondition(condition, context, _labelGenerator);
            case IfStatementNode ifStmt:
                return GenerateIfStatement(ifStmt, context, _labelGenerator);
            case WhileStatementNode whileStmt:
                return GenerateWhileStatement(whileStmt, context, _labelGenerator);
            case ReturnStatementNode returnStmt:
                return GenerateReturnStatement(returnStmt, context);
            default:
                throw new Exception($"Unhandled node {node}");
        }
    }

    private IReturnable? GenerateCall(CallExpressionNode node, CodeGenContext context)
    {
        // Save in use caller saved registers
        List<Register> toSave = context.Allocator.TrackedCallerSavedRegisters.Where(x => x.InUse).ToList();
        foreach (Register reg in toSave)
        {
            context.CodeGen.EmitPush(new ReadonlyRegister(reg));
        }

        List<Register> registersToFree = new();
        // Put function arguments in the right registers
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            IReturnable argument = Generate(node.Arguments[i], context) ?? throw new Exception("Argument was not set");
            RegisterEnum argumentSlotEnum = Allocator.ArgumentRegisters.ElementAt(i);
            if (argument is not ReturnRegister reg || reg.RegisterEnum != argumentSlotEnum)
            {
                Register argumentSlot = context.Allocator.Use(argumentSlotEnum, true);
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

        return new ReturnRegister(context.Allocator.Use(RegisterEnum.r0));
    }

    private IReturnable? GenerateBinary(BinaryExpressionNode node, CodeGenContext context)
    {
        IReturnable leftOperand = Generate(node.Left, context) ?? throw new Exception("Left operand of binary expression is missing");
        IReturnable rightOperand = Generate(node.Right, context) ?? throw new Exception("Right operand of binary expression is missing");

        if (leftOperand is NumberLiteralNode && rightOperand is NumberLiteralNode)
        {
            return new SimpleBinaryExpressionNode(node);
        }

        Operation op = node.Operation;
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(op, leftOperand, rightOperand, result);
        leftOperand.Free();
        rightOperand.Free();

        return new ReturnRegister(result);
    }

    private IReturnable? GenerateVariableDeclaration(VariableDeclarationNode node, CodeGenContext context)
    {
        int offset = context.AllocateVariable(node.Name);

        if (node.Initializer == null) return null;

        return AssignVariable(offset, node.Initializer, context);
    }

    private IReturnable? GenerateVariableAssignment(VariableAssignmentNode node, CodeGenContext context)
    {
        int offset = context.VariableTable[node.Name].FramePointerOffset;

        return AssignVariable(offset, node.Value, context);
    }

    private IReturnable? AssignVariable(int offset, AstNode assignment, CodeGenContext context)
    {
        IReturnable value = Generate(assignment, context) ?? throw new Exception("Initializer expression failed to generate");
        Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);

        context.CodeGen.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, offset.ToString()),
            tmp
        );
        context.CodeGen.EmitStore(value, new ReadonlyRegister(tmp));

        tmp.Free();
        value.Free();

        return value;
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

        throw new CodeGenException($"Undefined identifier was used", node.IdentifierToken, context.CompilerOptions, context.CompilerContext);
    }

    private IReturnable? GenerateCondition(ConditionNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string label = labelGenerator.Generate(LabelType.Condition);
        string endLabel = labelGenerator.Generate(LabelType.ConditionEnd);

        IReturnable left = Generate(node.Left, context) ?? throw new Exception("Part of if statement was not set");
        IReturnable right = Generate(node.Right, context) ?? throw new Exception("Part of if statement was not set");
        context.CodeGen.EmitIf(node.Comparer.GetOpposite(), left, right, label);
        left.Free();
        right.Free();

        Register resultRegister = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        // If {
        context.CodeGen.EmitMov(new NumberLiteralNode(1), resultRegister);
        // }
        context.CodeGen.EmitJump(endLabel);
        context.CodeGen.EmitLabel(label);
        // Else {
        context.CodeGen.EmitMov(new NumberLiteralNode(0), resultRegister);
        // }
        context.CodeGen.EmitLabel(endLabel);

        return new ReturnRegister(resultRegister);
    }

    private IReturnable? GenerateIfStatement(IfStatementNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string label;
        string? endLabel = null;
        if (node.ElseBody == null)
        {
            label = labelGenerator.Generate(LabelType.If);
        }
        else
        {
            label = labelGenerator.Generate(LabelType.Else);
            endLabel = labelGenerator.Generate(LabelType.IfElseEnd);
        }

        if (node.Condition is ConditionNode condition)
        {
            IReturnable left = Generate(condition.Left, context) ?? throw new Exception("Part of if statement was not set");
            IReturnable right = Generate(condition.Right, context) ?? throw new Exception("Part of if statement was not set");
            context.CodeGen.EmitIf(condition.Comparer.GetOpposite(), left, right, label);
            left.Free();
            right.Free();
        }
        else
        {
            IReturnable conditionResult = Generate(node.Condition, context) ?? throw new Exception("If condition didn't resolve to anything");

            if (conditionResult is ReturnRegister reg)
            {
                context.CodeGen.EmitIf(Comparer.IfEq, reg, new NumberLiteralNode(0), label);
            }
            else if (conditionResult is NumberLiteralNode num)
            {
                if (num.Value == "0")
                {
                    context.CodeGen.EmitJump(label);
                }
            }
            else if (conditionResult is BooleanLiteralNode boolean)
            {
                if (!boolean.Value)
                {
                    context.CodeGen.EmitJump(label);
                }
            }
            else
            {
                throw new Exception("Invalid condition result type");
            }
        }

        foreach (AstNode stmt in node.Body)
        {
            context.ExprFactory.Generate(stmt, (CodeGenContext)context.Clone());
        }

        if (endLabel != null)
        {
            context.CodeGen.EmitJump(endLabel);
        }

        context.CodeGen.EmitLabel(label);

        if (node.ElseBody != null)
        {
            foreach (AstNode stmt in node.ElseBody)
            {
                context.ExprFactory.Generate(stmt, (CodeGenContext)context.Clone());
            }
            context.CodeGen.EmitLabel(endLabel!);
        }
        return null;
    }

    private IReturnable? GenerateWhileStatement(WhileStatementNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string label;
        label = labelGenerator.Generate(LabelType.While);

        context.CodeGen.EmitLabel(label);

        foreach (AstNode stmt in node.Body)
        {
            context.ExprFactory.Generate(stmt, (CodeGenContext)context.Clone());
        }

        if (node.Condition is ConditionNode condition)
        {
            IReturnable left = Generate(condition.Left, context) ?? throw new Exception("Part of while statement was not set");
            IReturnable right = Generate(condition.Right, context) ?? throw new Exception("Part of while statement was not set");
            context.CodeGen.EmitIf(condition.Comparer, left, right, label);
            left.Free();
            right.Free();
        }
        else
        {
            IReturnable conditionResult = Generate(node.Condition, context) ?? throw new Exception("While condition didn't resolve to anything");

            if (conditionResult is ReturnRegister reg)
            {
                context.CodeGen.EmitIf(Comparer.IfNotEq, reg, new NumberLiteralNode(0), label);
            }
            else if (conditionResult is NumberLiteralNode num)
            {
                if (num.Value != "0")
                {
                    context.CodeGen.EmitJump(label);
                }
            }
            else if (conditionResult is BooleanLiteralNode boolean)
            {
                if (boolean.Value)
                {
                    context.CodeGen.EmitJump(label);
                }
            }
            else
            {
                throw new Exception("Invalid condition result type");
            }
        }

        return null;
    }

    private IReturnable? GenerateReturnStatement(ReturnStatementNode node, CodeGenContext context)
    {
        IReturnable returnValue = Generate(node.ReturnValue, context) ?? throw new Exception("return value didn't return anything");
        Register r0 = context.Allocator.Use(RegisterEnum.r0, true);
        context.CodeGen.EmitMov(returnValue, r0);
        returnValue.Free();
        r0.Free();
        context.CodeGen.EmitReturn(false);

        return returnValue;
    }
}

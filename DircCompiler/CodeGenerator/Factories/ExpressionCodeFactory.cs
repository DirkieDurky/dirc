using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;
using DircCompiler.Semantic;

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
            case ArrayDeclarationNode arrayDecl:
                return GenerateArrayDeclaration(arrayDecl, context);
            case ArrayLiteralNode:
                // Array literals should be handled by the receiver of the array
                return null;
            case ArrayAccessNode arrayAccess:
                return GenerateArrayAccess(arrayAccess, context);
            case ArrayAssignmentNode arrayAssign:
                return GenerateArrayAssignment(arrayAssign, context);
            case AddressOfNode addressOf:
                return GenerateAddressOf(addressOf, context);
            case PointerDereferenceNode deref:
                return GeneratePointerDereference(deref, context);
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
        if (node.Target is IdentifierNode)
        {
            int offset = context.VariableTable[node.Name!].FramePointerOffset;

            return AssignVariable(offset, node.Value, context);
        }
        else if (node.Target is PointerDereferenceNode target)
        {
            IReturnable address = Generate(target.PointerExpression, context) ?? throw new Exception("Pointer dereference failed to generate");
            IReturnable value = Generate(node.Value, context) ?? throw new Exception("Initializer expression failed to generate");

            context.CodeGen.EmitStore(value, address);
            address.Free();
            value.Free();

            return value;
        }
        else
        {
            throw new CodeGenException("Invalid node type of left side of variable assignment", node.TargetName, context.CompilerOptions, context.CompilerContext);
        }
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

    private IReturnable? GenerateArrayDeclaration(ArrayDeclarationNode node, CodeGenContext context)
    {
        IReturnable sizeResult = Generate(node.Size, context) ?? throw new Exception("Array size expression failed to generate");

        if (sizeResult is NumberLiteralNode sizeNode && int.TryParse(sizeNode.Value, out int size))
        {
            int baseOffset = context.AllocateArray(node.Name, size);

            if (node.Initializer != null)
            {
                return GenerateArrayInitialization(node.Name, node.Initializer, context);
            }
        }
        else
        {
            throw new CodeGenException("Invalid array size specified", node.IdentifierToken, context.CompilerOptions, context.CompilerContext);
        }

        sizeResult.Free();
        return null;
    }

    private IReturnable? GenerateArrayInitialization(string arrayName, AstNode initializer, CodeGenContext context)
    {
        if (initializer is not ArrayLiteralNode arrayLiteral) return null;

        if (!context.VariableTable.TryGetValue(arrayName, out Variable? variable))
        {
            throw new CodeGenException($"Undefined array '{arrayName}'", null, context.CompilerOptions, context.CompilerContext);
        }

        for (int i = 0; i < arrayLiteral.Elements.Count; i++)
        {
            IReturnable elementValue = Generate(arrayLiteral.Elements[i], context) ?? throw new Exception("Array element failed to generate");

            Register basePtr = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            context.CodeGen.EmitBinaryOperation(
                Operation.Sub,
                ReadonlyRegister.FP,
                new NumberLiteralNode(NumberLiteralType.Decimal, variable.FramePointerOffset.ToString()),
                basePtr
            );

            Register address = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            context.CodeGen.EmitBinaryOperation(
                Operation.Add,
                new ReadonlyRegister(basePtr),
                new NumberLiteralNode(NumberLiteralType.Decimal, i.ToString()),
                address
            );
            basePtr.Free();

            context.CodeGen.EmitStore(elementValue, new ReadonlyRegister(address));

            elementValue.Free();
            address.Free();
        }

        return null;
    }

    private IReturnable GenerateArrayAccess(ArrayAccessNode node, CodeGenContext context)
    {
        if (node.ArrayIsPointer!) return GenerateArrayPointerAccess(node, context);

        if (!context.VariableTable.TryGetValue(node.ArrayName, out Variable? variable))
        {
            throw new CodeGenException($"Undefined array '{node.ArrayName}'", node.ArrayToken, context.CompilerOptions, context.CompilerContext);
        }

        IReturnable indexResult = Generate(node.Index, context) ?? throw new Exception("Array index expression failed to generate");

        // Calculate the address: base + index
        Register basePtr = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, variable.FramePointerOffset.ToString()),
            basePtr
        );

        Register address = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(
            Operation.Add,
            new ReadonlyRegister(basePtr),
            indexResult,
            address
        );

        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitLoad(new ReadonlyRegister(address), result);

        indexResult.Free();
        basePtr.Free();
        address.Free();

        return new ReturnRegister(result);
    }

    private IReturnable GenerateArrayPointerAccess(ArrayAccessNode node, CodeGenContext context)
    {
        IReturnable pointerValue = Generate(new IdentifierNode(node.ArrayToken, node.ArrayName), context) ?? throw new Exception("Pointer expression failed to generate");
        IReturnable indexValue = Generate(node.Index, context) ?? throw new Exception("Array index failed to generate");
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        Register additionResult = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(Operation.Add, pointerValue, indexValue, additionResult);
        pointerValue.Free();
        indexValue.Free();
        context.CodeGen.EmitLoad(new ReadonlyRegister(additionResult), result);
        additionResult.Free();
        return new ReturnRegister(result);
    }

    private IReturnable? GenerateArrayAssignment(ArrayAssignmentNode node, CodeGenContext context)
    {
        if (node.ArrayIsPointer) return GenerateArrayPointerAssignment(node, context);
        if (!context.VariableTable.TryGetValue(node.ArrayName, out Variable? variable))
        {
            throw new CodeGenException($"Undefined array '{node.ArrayName}'", node.ArrayToken, context.CompilerOptions, context.CompilerContext);
        }

        IReturnable valueResult = Generate(node.Value, context) ?? throw new Exception("Array assignment value failed to generate");

        IReturnable indexResult = Generate(node.Index, context) ?? throw new Exception("Array index expression failed to generate");

        // Calculate the address: base + index
        Register basePtr = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, variable.FramePointerOffset.ToString()),
            basePtr
        );

        Register address = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(
            Operation.Add,
            new ReadonlyRegister(basePtr),
            indexResult,
            address
        );
        basePtr.Free();
        indexResult.Free();

        context.CodeGen.EmitStore(valueResult, new ReadonlyRegister(address));

        valueResult.Free();
        address.Free();

        return valueResult;
    }

    private IReturnable? GenerateArrayPointerAssignment(ArrayAssignmentNode node, CodeGenContext context)
    {
        IReturnable pointerValue = Generate(new IdentifierNode(node.ArrayToken, node.ArrayName), context) ?? throw new Exception("Pointer expression failed to generate");
        IReturnable indexValue = Generate(node.Index, context) ?? throw new Exception("Array index failed to generate");
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        Register additionResult = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(Operation.Add, pointerValue, indexValue, additionResult);
        pointerValue.Free();
        indexValue.Free();

        IReturnable value = Generate(node.Value, context) ?? throw new Exception("Variable assignment value failed to generate");

        context.CodeGen.EmitStore(value, new ReadonlyRegister(additionResult));
        additionResult.Free();
        value.Free();
        result.Free();

        return new ReturnRegister(result);
    }

    private IReturnable GenerateAddressOf(AddressOfNode node, CodeGenContext context)
    {
        // Only support address of local variables for now
        if (!context.VariableTable.TryGetValue(node.Variable.Name, out Variable? variable))
        {
            throw new CodeGenException($"Undefined variable '{node.Variable.Name}' for address-of", node.Variable.IdentifierToken, context.CompilerOptions, context.CompilerContext);
        }
        Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, variable.FramePointerOffset.ToString()),
            tmp
        );
        return new ReturnRegister(tmp);
    }

    private IReturnable GeneratePointerDereference(PointerDereferenceNode node, CodeGenContext context)
    {
        IReturnable pointerValue = Generate(node.PointerExpression, context) ?? throw new Exception("Pointer expression failed to generate");
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitLoad(pointerValue, result);
        pointerValue.Free();
        return new ReturnRegister(result);
    }
}

using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class ArrayFactory
{
    public IReturnable? GenerateArrayDeclaration(ArrayDeclarationNode node, CodeGenContext context)
    {
        IReturnable sizeResult = context.ExprFactory.Generate(node.Size, context) ?? throw new Exception("Array size expression failed to generate");

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
            throw new CodeGenException("Invalid array size specified", node.IdentifierToken, context.BuildOptions, context.BuildContext);
        }

        sizeResult.Free();
        return null;
    }

    public IReturnable? GenerateArrayInitialization(string arrayName, AstNode initializer, CodeGenContext context)
    {
        if (initializer is not ArrayLiteralNode arrayLiteral) return null;

        if (!context.VariableTable.TryGetValue(arrayName, out Variable? variable))
        {
            throw new CodeGenException($"Undefined array '{arrayName}'", null, context.BuildOptions, context.BuildContext);
        }

        for (int i = 0; i < arrayLiteral.Elements.Count; i++)
        {
            IReturnable elementValue = context.ExprFactory.Generate(arrayLiteral.Elements[i], context) ?? throw new Exception("Array element failed to generate");

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

    public IReturnable GenerateArrayAccess(ArrayAccessNode node, CodeGenContext context)
    {
        if (node.ArrayIsPointer!) return GenerateArrayPointerAccess(node, context);

        if (!context.VariableTable.TryGetValue(node.ArrayName, out Variable? variable))
        {
            throw new CodeGenException($"Undefined array '{node.ArrayName}'", node.ArrayToken, context.BuildOptions, context.BuildContext);
        }

        IReturnable indexResult = context.ExprFactory.Generate(node.Index, context) ?? throw new Exception("Array index expression failed to generate");

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

    public IReturnable GenerateArrayPointerAccess(ArrayAccessNode node, CodeGenContext context)
    {
        IReturnable pointerValue = context.ExprFactory.Generate(new IdentifierNode(node.ArrayToken, node.ArrayName), context) ?? throw new Exception("Pointer expression failed to generate");
        IReturnable indexValue = context.ExprFactory.Generate(node.Index, context) ?? throw new Exception("Array index failed to generate");
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        Register additionResult = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(Operation.Add, pointerValue, indexValue, additionResult);
        pointerValue.Free();
        indexValue.Free();
        context.CodeGen.EmitLoad(new ReadonlyRegister(additionResult), result);
        additionResult.Free();
        return new ReturnRegister(result);
    }

    public IReturnable? GenerateArrayAssignment(ArrayAssignmentNode node, CodeGenContext context)
    {
        if (node.ArrayIsPointer) return GenerateArrayPointerAssignment(node, context);
        if (!context.VariableTable.TryGetValue(node.ArrayName, out Variable? variable))
        {
            throw new CodeGenException($"Undefined array '{node.ArrayName}'", node.ArrayToken, context.BuildOptions, context.BuildContext);
        }

        IReturnable valueResult = context.ExprFactory.Generate(node.Value, context) ?? throw new Exception("Array assignment value failed to generate");

        IReturnable indexResult = context.ExprFactory.Generate(node.Index, context) ?? throw new Exception("Array index expression failed to generate");

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

    public IReturnable? GenerateArrayPointerAssignment(ArrayAssignmentNode node, CodeGenContext context)
    {
        IReturnable pointerValue = context.ExprFactory.Generate(new IdentifierNode(node.ArrayToken, node.ArrayName), context) ?? throw new Exception("Pointer expression failed to generate");
        IReturnable indexValue = context.ExprFactory.Generate(node.Index, context) ?? throw new Exception("Array index failed to generate");
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        Register additionResult = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(Operation.Add, pointerValue, indexValue, additionResult);
        pointerValue.Free();
        indexValue.Free();

        IReturnable value = context.ExprFactory.Generate(node.Value, context) ?? throw new Exception("Variable assignment value failed to generate");

        context.CodeGen.EmitStore(value, new ReadonlyRegister(additionResult));
        additionResult.Free();
        value.Free();
        result.Free();

        return new ReturnRegister(result);
    }
}

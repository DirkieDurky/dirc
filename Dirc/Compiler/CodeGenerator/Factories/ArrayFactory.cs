using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class ArrayFactory
{
    private readonly CodeGenBase _codeGenBase;

    public ArrayFactory(CodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable? GenerateArrayDeclaration(ArrayDeclarationNode node, CodeGenContext context)
    {
        IReturnable sizeResult = context.ExprFactory.Generate(node.Size, context) ?? throw new Exception("Array size expression failed to generate");

        if (sizeResult is NumberLiteralNode sizeNode && int.TryParse(sizeNode.Value, out int size))
        {
            if (node.Initializer == null)
            {
                context.AllocateArray(5, node.Name);
            }
            else
            {
                return GenerateArrayInitialization(node.Name, node.Initializer, true, context);
            }
        }
        else
        {
            throw new CodeGenException("Invalid array size specified", node.IdentifierToken, context.Options, context.BuildContext);
        }

        sizeResult.Free();
        return null;
    }

    public IReturnable? GenerateArrayInitialization(string arrayName, AstNode initializer, bool isDeclaration, CodeGenContext context)
    {
        if (!isDeclaration)
        {
            if (context.VariableTable[arrayName] is StackStoredVariable)
            {
                //TODO: Free variable
            }
            else
            {
                throw new CodeGenException("Variable was of unsupported type", null, context.Options, context.BuildContext);
            }
        }

        switch (initializer)
        {
            case ArrayLiteralNode arrayLiteralNode:
                {
                    ArrayLiteral arrayLiteral = GenerateArrayLiteral(arrayLiteralNode, context);
                    context.AssignNameToArray(arrayLiteral.Offset, arrayName);

                    return new ReturnRegister(arrayLiteral.BasePtr);
                }
            case StringLiteralNode stringLiteralNode:
                {
                    List<AstNode> chars = new();
                    foreach (char c in stringLiteralNode.Str.Literal!.ToString()!)
                    {
                        chars.Add(new CharNode(c));
                    }
                    ArrayLiteralNode arrayLiteralNode = new(chars);
                    ArrayLiteral arrayLiteral = GenerateArrayLiteral(arrayLiteralNode, context);
                    context.AssignNameToArray(arrayLiteral.Offset, arrayName);

                    return new ReturnRegister(arrayLiteral.BasePtr);
                }
            default:
                throw new CodeGenException("Invalid node given as array initializer. Expected an array- or string literal.", null, context.Options, context.BuildContext);
        }
    }

    public ReturnRegister GenerateArrayLiteralReturnBasePtr(ArrayLiteralNode arrayLiteralNode, CodeGenContext context)
    {
        return new ReturnRegister(GenerateArrayLiteral(arrayLiteralNode, context).BasePtr);
    }

    public ArrayLiteral GenerateArrayLiteral(ArrayLiteralNode arrayLiteralNode, CodeGenContext context)
    {
        int offset = context.AllocateArray(arrayLiteralNode.Elements.Count, null);

        Register basePtr = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, offset.ToString()),
            basePtr
        );
        ReadonlyRegister readOnlyBasePtr = new(basePtr);

        for (int i = 0; i < arrayLiteralNode.Elements.Count; i++)
        {
            IReturnable elementValue = context.ExprFactory.Generate(arrayLiteralNode.Elements[i], context) ?? throw new Exception("Array element failed to generate");

            Register address = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            _codeGenBase.EmitBinaryOperation(
                Operation.Add,
                readOnlyBasePtr,
                new NumberLiteralNode(NumberLiteralType.Decimal, i.ToString()),
                address
            );

            _codeGenBase.EmitStore(elementValue, new ReadonlyRegister(address));

            elementValue.Free();
            address.Free();
        }

        return new(offset, basePtr);
    }

    public IReturnable GenerateArrayAccess(ArrayAccessNode node, CodeGenContext context)
    {
        if (node.ArrayIsPointer!) return GenerateArrayPointerAccess(node, context);

        if (context.VariableTable[node.ArrayName] is not StackStoredVariable stackVar)
        {
            throw new CodeGenException("Variable was of unsupported type", null, context.Options, context.BuildContext);
        }

        IReturnable indexResult = context.ExprFactory.Generate(node.Index, context) ?? throw new Exception("Array index expression failed to generate");

        // Calculate the address: base + index
        Register basePtr = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, stackVar.FramePointerOffset.ToString()),
            basePtr
        );

        Register address = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitBinaryOperation(
            Operation.Add,
            new ReadonlyRegister(basePtr),
            indexResult,
            address
        );

        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitLoad(new ReadonlyRegister(address), result);

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
        _codeGenBase.EmitBinaryOperation(Operation.Add, pointerValue, indexValue, additionResult);
        pointerValue.Free();
        indexValue.Free();
        _codeGenBase.EmitLoad(new ReadonlyRegister(additionResult), result);
        additionResult.Free();
        return new ReturnRegister(result);
    }

    public IReturnable? GenerateArrayAssignment(ArrayAssignmentNode node, CodeGenContext context)
    {
        if (node.ArrayIsPointer) return GenerateArrayPointerAssignment(node, context);
        if (context.VariableTable[node.ArrayName] is not StackStoredVariable stackVar)
        {
            throw new CodeGenException("Variable was of unsupported type", null, context.Options, context.BuildContext);
        }

        IReturnable valueResult = context.ExprFactory.Generate(node.Value, context) ?? throw new Exception("Array assignment value failed to generate");

        IReturnable indexResult = context.ExprFactory.Generate(node.Index, context) ?? throw new Exception("Array index expression failed to generate");

        // Calculate the address: base + index
        Register basePtr = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, stackVar.FramePointerOffset.ToString()),
            basePtr
        );

        Register address = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitBinaryOperation(
            Operation.Add,
            new ReadonlyRegister(basePtr),
            indexResult,
            address
        );
        basePtr.Free();
        indexResult.Free();

        _codeGenBase.EmitStore(valueResult, new ReadonlyRegister(address));

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
        _codeGenBase.EmitBinaryOperation(Operation.Add, pointerValue, indexValue, additionResult);
        pointerValue.Free();
        indexValue.Free();

        IReturnable value = context.ExprFactory.Generate(node.Value, context) ?? throw new Exception("Variable assignment value failed to generate");

        _codeGenBase.EmitStore(value, new ReadonlyRegister(additionResult));
        additionResult.Free();
        value.Free();
        result.Free();

        return new ReturnRegister(result);
    }
}

class ArrayLiteral(int offset, Register basePtr)
{
    public int Offset = offset;
    public Register BasePtr = basePtr;
}

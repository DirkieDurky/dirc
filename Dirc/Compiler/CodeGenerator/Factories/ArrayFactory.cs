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
        if (node.Initializer == null)
        {
            context.AllocateStackArray(node.TotalSize(), node.Name);
        }
        else
        {
            return GenerateArrayInitialization(node.Name, node.Initializer, true, context);
        }

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
        int offset = context.AllocateStackArray(arrayLiteralNode.Elements.Count, null);

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
        IReturnable pointerValue = context.ExprFactory.Generate(node.Array, context) ?? throw new Exception("Pointer expression failed to generate");
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
        IReturnable pointerValue = context.ExprFactory.Generate(node.Array, context) ?? throw new Exception("Pointer expression failed to generate");
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

using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;
using Dirc.HAL;

namespace Dirc.Compiling.CodeGen;

class ArrayFactory
{
    private readonly ICodeGenBase _codeGenBase;

    public ArrayFactory(ICodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable? GenerateArrayDeclaration(ArrayDeclarationNode node, CodeGenContext context)
    {
        int offset = context.AllocateStackArray(node.Sizes[0]!, node.Name);
        context.AssignNameToArray(offset, node.Name);

        if (node.Initializer != null)
        {
            context.VarFactory.AssignVariable(node.Name, node.Initializer, context);
        }

        return null;
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
            new ReadonlyRegister(context.FP),
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

    public void GenerateArrayLiteralAtPtr(ArrayLiteralNode arrayLiteralNode, IOperand basePtr, CodeGenContext context)
    {
        for (int i = 0; i < arrayLiteralNode.Elements.Count; i++)
        {
            IReturnable elementValue = context.ExprFactory.Generate(arrayLiteralNode.Elements[i], context) ?? throw new Exception("Array element failed to generate");

            Register address = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            _codeGenBase.EmitBinaryOperation(
                Operation.Add,
                basePtr,
                new NumberLiteralNode(NumberLiteralType.Decimal, i.ToString()),
                address
            );

            _codeGenBase.EmitStore(elementValue, new ReadonlyRegister(address));

            elementValue.Free();
            address.Free();
        }
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

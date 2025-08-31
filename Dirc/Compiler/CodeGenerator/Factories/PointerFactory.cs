using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class PointerFactory
{
    private readonly CodeGenBase _codeGenBase;

    public PointerFactory(CodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable GenerateAddressOf(AddressOfNode node, CodeGenContext context)
    {
        // Only support address of local variables for now
        if (!context.VariableTable.TryGetValue(node.Variable.Name, out Variable? variable))
        {
            throw new CodeGenException($"Undefined variable '{node.Variable.Name}' for address-of", node.Variable.IdentifierToken, context.BuildOptions, context.BuildContext);
        }
        Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, variable.FramePointerOffset.ToString()),
            tmp
        );
        return new ReturnRegister(tmp);
    }

    public IReturnable GeneratePointerDereference(PointerDereferenceNode node, CodeGenContext context)
    {
        IReturnable pointerValue = context.ExprFactory.Generate(node.PointerExpression, context) ?? throw new Exception("Pointer expression failed to generate");
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitLoad(pointerValue, result);
        pointerValue.Free();
        return new ReturnRegister(result);
    }
}

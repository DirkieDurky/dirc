using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class PointerFactory
{
    public IReturnable GenerateAddressOf(AddressOfNode node, CodeGenContext context)
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

    public IReturnable GeneratePointerDereference(PointerDereferenceNode node, CodeGenContext context)
    {
        IReturnable pointerValue = context.ExprFactory.Generate(node.PointerExpression, context) ?? throw new Exception("Pointer expression failed to generate");
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitLoad(pointerValue, result);
        pointerValue.Free();
        return new ReturnRegister(result);
    }
}

using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class IdentifierFactory
{
    private readonly CodeGenBase _codeGenBase;

    public IdentifierFactory(CodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable Generate(IdentifierNode node, CodeGenContext context)
    {
        if (context.RegisterTable.TryGetValue(node.Name, out Register? reg))
        {
            return new ReturnRegister(reg);
        }

        if (context.VariableTable.TryGetValue(node.Name, out Variable? variable))
        {
            if (variable.IsArray)
            {
                return context.PointerFactory.GenerateAddressOf(new AddressOfNode(node), context);
            }

            Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            _codeGenBase.EmitBinaryOperation(
                Operation.Sub,
                ReadonlyRegister.FP,
                new NumberLiteralNode(variable.FramePointerOffset * CodeGenContext.StackAlignment),
                tmp
            );

            Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            _codeGenBase.EmitLoad(new ReadonlyRegister(tmp), result);
            tmp.Free();

            return new ReturnRegister(result);
        }

        throw new CodeGenException($"Undefined identifier was used", node.IdentifierToken, context.BuildOptions, context.BuildContext);
    }
}

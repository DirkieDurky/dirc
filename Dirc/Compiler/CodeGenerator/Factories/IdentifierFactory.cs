using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;
using Dirc.HAL;

namespace Dirc.Compiling.CodeGen;

class IdentifierFactory
{
    private readonly ICodeGenBase _codeGenBase;

    public IdentifierFactory(ICodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable Generate(IdentifierNode node, CodeGenContext context)
    {
        Variable var = context.VariableTable[node.Name];

        if (var is RegisterStoredVariable regVar)
        {
            return new ReturnRegister(regVar.Register);
        }
        else if (var is StackStoredVariable stackVar)
        {
            if (stackVar.IsArray)
            {
                return context.PointerFactory.GenerateAddressOf(new AddressOfNode(node), context);
            }

            Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            _codeGenBase.EmitBinaryOperation(
                Operation.Sub,
                new ReadonlyRegister(context.FP),
                new NumberLiteralNode(stackVar.FramePointerOffset * context.Options.TargetArchitecture.StackAlignment),
                tmp
            );

            Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
            _codeGenBase.EmitLoad(new ReadonlyRegister(tmp), result);
            tmp.Free();

            return new ReturnRegister(result);
        }
        else if (var is DirectVariable directVar)
        {
            return new NumberLiteralNode(NumberLiteralType.Decimal, directVar.Value.ToString());
        }
        else
        {
            throw new CodeGenException("Variable was of unsupported type", null, context.Options, context.BuildContext);
        }
    }
}

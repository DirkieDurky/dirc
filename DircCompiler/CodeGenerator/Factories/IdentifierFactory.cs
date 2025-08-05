using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class IdentifierFactory
{
    public IReturnable Generate(IdentifierNode node, CodeGenContext context)
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
}

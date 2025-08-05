using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class CallFactory
{
    public IReturnable? Generate(CallExpressionNode node, CodeGenContext context)
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
            IReturnable argument = context.ExprFactory.Generate(node.Arguments[i], context) ?? throw new Exception("Argument was not set");
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
}

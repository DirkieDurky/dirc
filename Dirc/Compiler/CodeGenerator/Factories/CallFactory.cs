using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class CallFactory
{
    private readonly CodeGenBase _codeGenBase;

    public CallFactory(CodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable? Generate(CallExpressionNode node, CodeGenContext context)
    {
        // Save in use caller saved registers
        List<Register> toSave = context.Allocator.TrackedCallerSavedRegisters.Where(x => x.InUse).ToList();
        foreach (Register reg in toSave)
        {
            _codeGenBase.EmitPush(new ReadonlyRegister(reg));
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
                _codeGenBase.EmitMov(argument, argumentSlot);
            }
            argument.Free();
        }

        _codeGenBase.EmitFunctionCall(node.Callee, !context.DeclaredFunctions.Contains(node.Callee));

        foreach (Register reg in registersToFree)
        {
            reg.Free();
        }

        // Allocate space for the toSave values
        foreach (Register reg in toSave)
        {
            context.Allocator.Use(reg.RegisterEnum, true);
        }

        // Move the return value to a register that won't be overwritten by popping all the values
        Register returnValue = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitMov(ReadonlyRegister.R0, returnValue);

        // Restore saved values (and put them in the same registers they were in before)
        toSave.Reverse();
        foreach (Register reg in toSave)
        {
            _codeGenBase.EmitPop(reg);
        }

        return new ReturnRegister(returnValue);
    }
}

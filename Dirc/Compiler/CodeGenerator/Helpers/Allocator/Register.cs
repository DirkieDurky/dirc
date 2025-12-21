using Dirc.HAL;

namespace Dirc.Compiling.CodeGen.Allocating;

public class Register
{
    public RegisterBase RegisterBase { get; }
    public bool InUse = false;
    public bool RefersToFunctionArgument = false;

    private Allocator _allocator;

    public Register(Allocator allocator, RegisterBase register, bool refersToFunctionArgument = false)
    {
        _allocator = allocator;
        RegisterBase = register;
        RefersToFunctionArgument = refersToFunctionArgument;
    }

    public override string ToString()
    {
        return RegisterBase.Name;
    }

    public void Free()
    {
        if (RefersToFunctionArgument) return;
        InUse = false;
        if (_allocator.Options.CheckDebugOption(DebugOption.Allocator))
        {
            Console.Write($"Freed register {this} ");
            Allocator.StackTrace(1, 1);
        }
    }

    public Register Clone()
    {
        Register newReg = new Register(_allocator, RegisterBase, RefersToFunctionArgument);
        newReg.InUse = InUse;

        return newReg;
    }

    public static bool operator ==(Register ob1, Register ob2)
    {
        return ob1.RegisterBase.Equals(ob2.RegisterBase);
    }

    public static bool operator !=(Register ob1, Register ob2)
    {
        return !ob1.RegisterBase.Equals(ob2.RegisterBase);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Register reg) return false;
        return RegisterBase.Equals(reg.RegisterBase);
    }

    public override int GetHashCode()
    {
        return RegisterBase.ID;
    }
}

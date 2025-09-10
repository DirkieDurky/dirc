namespace Dirc.Compiling.CodeGen.Allocating;

public class Register
{
    public RegisterEnum RegisterEnum { get; }
    public bool InUse = false;
    public bool RefersToFunctionArgument = false;

    private Allocator _allocator;

    public Register(Allocator allocator, RegisterEnum register, bool refersToFunctionArgument = false)
    {
        _allocator = allocator;
        RegisterEnum = register;
        RefersToFunctionArgument = refersToFunctionArgument;
    }

    public override string ToString()
    {
        return RegisterEnum.ToString();
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
}

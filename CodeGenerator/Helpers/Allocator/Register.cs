namespace Dirc.CodeGen.Allocating;

class Register
{
    public RegisterEnum RegisterEnum { get; }
    public bool InUse = false;

    private Allocator _allocator;

    public Register(Allocator allocator, RegisterEnum register)
    {
        _allocator = allocator;
        RegisterEnum = register;
    }

    public override string ToString()
    {
        return RegisterEnum.ToString();
    }

    public void Free()
    {
        InUse = false;
        if (_allocator.CompilerOptions.LogAllocation)
        {
            Console.Write($"Freed register {this} ");
            Allocator.StackTrace(1, 1);
        }
    }
}

using Dirc.HAL;

namespace Dirc.Compiling.CodeGen.Allocating;

class ReadonlyRegister : IOperand
{
    public RegisterBase RegisterBase { get; }

    public ReadonlyRegister(RegisterBase register)
    {
        RegisterBase = register;
    }

    public ReadonlyRegister(Register register)
    {
        RegisterBase = register.RegisterBase;
    }

    public string AsOperand() => RegisterBase.Name;

    public void Free()
    {
        // Doesn't need to be freed
    }
}

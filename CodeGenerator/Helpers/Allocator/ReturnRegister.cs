namespace Dirc.CodeGen.Allocating;

class ReturnRegister : IReturnable
{
    public Register Register { get; }

    public ReturnRegister(Register register)
    {
        Register = register;
    }

    public string AsOperand() => Register.ToString();

    public void Free()
    {
        Register.Free();
    }

    public RegisterEnum RegisterEnum => Register.RegisterEnum;
}

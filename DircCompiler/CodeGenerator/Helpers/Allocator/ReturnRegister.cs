namespace DircCompiler.CodeGen.Allocating;

class ReturnRegister : IReturnable
{
    public Register Register
    {
        get
        {
            if (!_register.InUse) throw new Exception($"Register {_register.RegisterEnum} was used while free");
            return _register;
        }
        private set => _register = value;
    }

    private Register _register;

    public ReturnRegister(Register register)
    {
        _register = register;
    }

    public RegisterEnum RegisterEnum => _register.RegisterEnum;

    public string AsOperand() => Register.ToString();

    public void Free()
    {
        _register.Free();
    }
}

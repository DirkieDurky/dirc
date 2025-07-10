class Register : IOperand
{
    public RegisterEnum RegisterEnum { get; }
    public Register(RegisterEnum register) => RegisterEnum = register;
    public string AsOperand() => RegisterEnum.ToString();
}

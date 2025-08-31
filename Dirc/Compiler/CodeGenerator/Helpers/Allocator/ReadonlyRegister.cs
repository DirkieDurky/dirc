namespace Dirc.Compiling.CodeGen.Allocating;

class ReadonlyRegister : IOperand
{
    public RegisterEnum Register { get; }

    public ReadonlyRegister(RegisterEnum register)
    {
        Register = register;
    }

    public ReadonlyRegister(Register register)
    {
        Register = register.RegisterEnum;
    }

    public string AsOperand() => Register.ToString();

    public void Free()
    {
        // Doesn't need to be freed
    }

    public static ReadonlyRegister R0 => new(RegisterEnum.r0);
    public static ReadonlyRegister R1 => new(RegisterEnum.r1);
    public static ReadonlyRegister R2 => new(RegisterEnum.r2);
    public static ReadonlyRegister R3 => new(RegisterEnum.r3);
    public static ReadonlyRegister R4 => new(RegisterEnum.r4);
    public static ReadonlyRegister R5 => new(RegisterEnum.r5);
    public static ReadonlyRegister R6 => new(RegisterEnum.r6);
    public static ReadonlyRegister R7 => new(RegisterEnum.r7);
    public static ReadonlyRegister R8 => new(RegisterEnum.r8);
    public static ReadonlyRegister R9 => new(RegisterEnum.r9);
    public static ReadonlyRegister R10 => new(RegisterEnum.r10);
    public static ReadonlyRegister FP => new(RegisterEnum.fp);
    public static ReadonlyRegister SP => new(RegisterEnum.sp);
    public static ReadonlyRegister LR => new(RegisterEnum.lr);
}

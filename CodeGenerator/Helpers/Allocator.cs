using System.Diagnostics;

class Allocator
{
    public static IReadOnlyCollection<Register> ArgumentRegisters = [Register.R0, Register.R1, Register.R2, Register.R3];
    public static IReadOnlyCollection<Register> CallerSaved = [Register.R0, Register.R1, Register.R2, Register.R3, Register.R4, Register.R5];
    public static IReadOnlyCollection<Register> CalleeSaved = [Register.R6, Register.R7, Register.R8, Register.R9, Register.R10];

    public HashSet<Register> InUse = new();

    public Register Allocate(RegisterType type)
    {
        Register? foundRegister;
        if (type == RegisterType.CallerSaved)
        {
            foundRegister = CallerSaved.Any(r => !InUse.Contains(r)) ? CallerSaved.First(r => !InUse.Contains(r)) : null;
        }
        else
        {
            foundRegister = CalleeSaved.Any(r => !InUse.Contains(r)) ? CalleeSaved.First(r => !InUse.Contains(r)) : null;
        }
        if (foundRegister == null) throw new Exception("No free register to allocate.");

        Register register = foundRegister;
        InUse.Add(register);
        Debug.WriteLine($"Allocated register {register}{Environment.StackTrace}");
        return register;
    }

    public void Free(Register r)
    {
        InUse.Remove(r);
        Debug.WriteLine($"Freed register {r}");
    }

    public void Use(Register r)
    {
        if (InUse.Contains(r)) throw new Exception($"May not use {r}. Register is already in use");
        InUse.Add(r);
    }

    public enum RegisterType
    {
        CallerSaved,
        CalleeSaved,
    }
}

public enum RegisterEnum
{
    r0,
    r1,
    r2,
    r3,
    r4,
    r5,
    r6,
    r7,
    r8,
    r9,
    r10,
    fp,
    sp,
    lr,
}

class Register : IOperand
{
    public RegisterEnum RegisterEnum { get; }
    public Register(RegisterEnum register) => RegisterEnum = register;
    public string AsOperand() => RegisterEnum.ToString();

    public static Register R0 => new Register(RegisterEnum.r0);
    public static Register R1 => new Register(RegisterEnum.r1);
    public static Register R2 => new Register(RegisterEnum.r2);
    public static Register R3 => new Register(RegisterEnum.r3);
    public static Register R4 => new Register(RegisterEnum.r4);
    public static Register R5 => new Register(RegisterEnum.r5);
    public static Register R6 => new Register(RegisterEnum.r6);
    public static Register R7 => new Register(RegisterEnum.r7);
    public static Register R8 => new Register(RegisterEnum.r8);
    public static Register R9 => new Register(RegisterEnum.r9);
    public static Register R10 => new Register(RegisterEnum.r10);
    public static Register FP => new Register(RegisterEnum.fp);
    public static Register SP => new Register(RegisterEnum.sp);
    public static Register LR => new Register(RegisterEnum.lr);
}

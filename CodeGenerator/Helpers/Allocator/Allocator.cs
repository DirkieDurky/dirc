using System.Diagnostics;

namespace Dirc.CodeGen.Allocating;

class Allocator
{
    public static IReadOnlyCollection<RegisterEnum> ArgumentRegisters = [RegisterEnum.r0, RegisterEnum.r1, RegisterEnum.r2, RegisterEnum.r3];
    public static IReadOnlyCollection<RegisterEnum> CallerSavedRegisters = [RegisterEnum.r0, RegisterEnum.r1, RegisterEnum.r2, RegisterEnum.r3, RegisterEnum.r4, RegisterEnum.r5];
    public static IReadOnlyCollection<RegisterEnum> CalleeSavedRegisters = [RegisterEnum.r6, RegisterEnum.r7, RegisterEnum.r8, RegisterEnum.r9, RegisterEnum.r10];

    public IReadOnlyCollection<Register> TrackedCallerSavedRegisters { get; }
    public IReadOnlyCollection<Register> TrackedCalleeSavedRegisters { get; }

    public Allocator()
    {
        Register[] callerSaved = new Register[CallerSavedRegisters.Count];
        for (int i = 0; i < CallerSavedRegisters.Count; i++)
        {
            callerSaved[i] = new Register(this, CallerSavedRegisters.ElementAt(i));
        }
        TrackedCallerSavedRegisters = callerSaved;
        Register[] calleeSaved = new Register[CalleeSavedRegisters.Count];
        for (int i = 0; i < CalleeSavedRegisters.Count; i++)
        {
            calleeSaved[i] = new Register(this, CalleeSavedRegisters.ElementAt(i));
        }
        TrackedCalleeSavedRegisters = calleeSaved;
    }

    public Register Allocate(RegisterType type)
    {
        Register? foundRegister;
        if (type == RegisterType.CallerSaved)
        {
            foundRegister = TrackedCallerSavedRegisters.First(r => !r.InUse);
        }
        else
        {
            foundRegister = TrackedCalleeSavedRegisters.First(r => !r.InUse);
        }
        if (foundRegister == null) throw new Exception("No free register to allocate.");

        Register register = foundRegister;
        register.InUse = true;
        // Console.WriteLine($"Allocated register {register}");
        // StackTrace(1, 1);

        return register;
    }

    public Register Use(RegisterEnum r)
    {
        Register foundRegister = GetRegisterFromEnum(r);

        if (foundRegister.InUse) throw new Exception($"May not use {r}. Register is already in use");

        foundRegister.InUse = true;
        return foundRegister;
    }

    public Register GetRegisterFromEnum(RegisterEnum r)
    {
        if (r == RegisterEnum.fp || r == RegisterEnum.sp || r == RegisterEnum.lr) return new Register(this, r);

        if (CallerSavedRegisters.Contains(r))
        {
            return TrackedCallerSavedRegisters.First(x => x.RegisterEnum == r);
        }
        else if (CalleeSavedRegisters.Contains(r))
        {
            return TrackedCalleeSavedRegisters.First(x => x.RegisterEnum == r);
        }
        else
        {
            throw new Exception("Specified register not found");
        }
    }

    public enum RegisterType
    {
        CallerSaved,
        CalleeSaved,
    }


    static void StackTrace(int skipMethods, int amount)
    {
        var stackTrace = new StackTrace(skipFrames: skipMethods + 1, fNeedFileInfo: true);
        foreach (var frame in stackTrace.GetFrames() ?? Array.Empty<StackFrame>())
        {
            System.Reflection.MethodBase method = frame.GetMethod()!;
            if (method.Name == "Free") continue;
            Console.WriteLine($"at {method.DeclaringType}.{method.Name} in {frame.GetFileName()}:line {frame.GetFileLineNumber()}");
            if (--amount <= 0) return;
        }
    }
}

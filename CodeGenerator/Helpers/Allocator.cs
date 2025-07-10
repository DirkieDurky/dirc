using System.Collections.Immutable;

static class Allocator
{
    public static IReadOnlyCollection<RegisterEnum> ArgumentRegisters = [RegisterEnum.r0, RegisterEnum.r1];
    public static IReadOnlyCollection<RegisterEnum> CallerSaved = [RegisterEnum.r0, RegisterEnum.r1];
    public static IReadOnlyCollection<RegisterEnum> CalleeSaved = [RegisterEnum.r2, RegisterEnum.r3];

    public static HashSet<RegisterEnum> InUse = new();

    public static RegisterEnum Allocate(RegisterType type)
    {
        RegisterEnum? foundRegister;
        if (type == RegisterType.CallerSaved)
        {
            foundRegister = CallerSaved.FirstOrDefault(r => !InUse.Contains(r));
        }
        else
        {
            foundRegister = CalleeSaved.FirstOrDefault(r => !InUse.Contains(r));
        }
        if (foundRegister == null) throw new Exception("No free register to allocate.");

        RegisterEnum register = (RegisterEnum)foundRegister;
        InUse.Add(register);
        return register;
    }

    public static void Free(RegisterEnum r)
    {
        InUse.Remove(r);
    }

    public static void Use(RegisterEnum r)
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

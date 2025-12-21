using System.Diagnostics;
using Dirc.HAL;

namespace Dirc.Compiling.CodeGen.Allocating;

public class Allocator : ICloneable
{
    public Options Options;
    public BuildContext BuildContext;

    public IReadOnlyCollection<Register> TrackedCallerSavedRegisters { get; private set; }
    public IReadOnlyCollection<Register> TrackedCalleeSavedRegisters { get; private set; }

    public Allocator(Options options, BuildContext buildContext)
    {
        RegisterBase[] callerSavedRegisters = options.TargetArchitecture.CallerSavedRegisters;
        RegisterBase[] calleeSavedRegisters = options.TargetArchitecture.CalleeSavedRegisters;

        Options = options;
        BuildContext = buildContext;

        Register[] callerSaved = new Register[callerSavedRegisters.Length];
        for (int i = 0; i < callerSavedRegisters.Length; i++)
        {
            callerSaved[i] = new Register(this, callerSavedRegisters.ElementAt(i));
        }
        TrackedCallerSavedRegisters = callerSaved;
        Register[] calleeSaved = new Register[calleeSavedRegisters.Length];
        for (int i = 0; i < calleeSavedRegisters.Length; i++)
        {
            calleeSaved[i] = new Register(this, calleeSavedRegisters.ElementAt(i));
        }
        TrackedCalleeSavedRegisters = calleeSaved;
    }

    public Register Allocate(RegisterType type)
    {
        Register? foundRegister;
        if (type == RegisterType.CallerSaved)
        {
            foundRegister = TrackedCallerSavedRegisters.FirstOrDefault(r => !r.InUse) ?? throw new CodeGenException("No more registers to allocate.", null, Options, BuildContext);
        }
        else
        {
            foundRegister = TrackedCalleeSavedRegisters.FirstOrDefault(r => !r.InUse) ?? throw new CodeGenException("No more registers to allocate.", null, Options, BuildContext);
        }

        Register register = foundRegister;
        register.InUse = true;

        if (Options.CheckDebugOption(DebugOption.Allocator))
        {
            Console.Write($"Allocated register {register} ");
            StackTrace(1, 1);
        }

        return register;
    }

    public Register Use(RegisterBase r, bool overwrite = false, bool forFunctionArgument = false)
    {
        Register foundRegister = GetRegisterFromEnum(r);

        if (!overwrite)
        {
            if (foundRegister.InUse) throw new Exception($"May not use {r}. Register is already in use");
        }

        if (Options.CheckDebugOption(DebugOption.Allocator))
        {
            Console.Write($"Allocated register {foundRegister} ");
            StackTrace(1, 1);
        }

        foundRegister.InUse = true;
        foundRegister.RefersToFunctionArgument = forFunctionArgument;
        return foundRegister;
    }

    public Register GetRegisterFromEnum(RegisterBase r)
    {
        if (r.AlwaysAvailable) return new Register(this, r);

        if (Options.TargetArchitecture.CallerSavedRegisters.Contains(r))
        {
            return TrackedCallerSavedRegisters.First(x => x.RegisterBase == r);
        }
        else if (Options.TargetArchitecture.CalleeSavedRegisters.Contains(r))
        {
            return TrackedCalleeSavedRegisters.First(x => x.RegisterBase == r);
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

    public static void StackTrace(int skipMethods, int amount)
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

    public object Clone()
    {
        Allocator newAllocator = new Allocator(Options, BuildContext);

        newAllocator.TrackedCallerSavedRegisters = CloneRegisterCollection(newAllocator, TrackedCallerSavedRegisters);
        newAllocator.TrackedCalleeSavedRegisters = CloneRegisterCollection(newAllocator, TrackedCalleeSavedRegisters);

        return newAllocator;
    }

    public static IReadOnlyCollection<Register> CloneRegisterCollection(Allocator newAllocator, IReadOnlyCollection<Register> collection)
    {
        List<Register> result = [];
        foreach (Register oldRegister in collection)
        {
            Register newRegister = new Register(newAllocator, oldRegister.RegisterBase, oldRegister.RefersToFunctionArgument);
            newRegister.InUse = oldRegister.InUse;
            result.Add(newRegister);
        }
        return result;
    }
}

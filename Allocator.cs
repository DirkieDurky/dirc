using System.Collections.Immutable;

class Allocator
{
    private ImmutableArray<Register> MayUse = [
        Register.r0,
        Register.r1,
        Register.r2,
        Register.r3,
    ];

    public List<Register> InUse = new();

    public Allocator(List<Register> inUse)
    {
        InUse = inUse;
    }

    public Register AllocateFirst()
    {
        Register? foundRegister = MayUse.FirstOrDefault(r => !InUse.Contains(r));
        if (foundRegister == null) throw new Exception("No free register to allocate.");

        Register register = (Register)foundRegister;
        InUse.Add(register);
        return register;
    }

    public void Free(Register r)
    {
        InUse.Remove(r);
    }
}

using Dirc.Compiling.CodeGen.Allocating;

namespace Dirc.Compiling.CodeGen;

public static class VariableTableExtensions
{
    public static Variable? GetByRegister(
        this Dictionary<string, Variable> table,
        Register reg)
    {
        foreach (var kvp in table)
        {
            if (kvp.Value is RegisterStoredVariable rsv &&
                rsv.Register.RegisterEnum == reg.RegisterEnum)
            {
                return rsv;
            }
        }

        return null;
    }
}

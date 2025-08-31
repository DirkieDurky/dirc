namespace Dirc.Compiling.CodeGen;

public class FunctionTable : ICloneable
{
    private readonly Dictionary<string, Function> _functions = new();

    private FunctionTable(Dictionary<string, Function> functions)
    {
        _functions = functions;
    }

    public object Clone()
    {
        return new FunctionTable(_functions.ToDictionary(x => x.Key, x => x.Value));
    }
}

namespace Dirc.CodeGen;

public class FunctionTable : ICloneable
{
    private readonly Dictionary<string, Function> _functions = new();

    public FunctionTable()
    {
    }

    private FunctionTable(Dictionary<string, Function> functions)
    {
        _functions = functions;
    }

    public void Declare(Function sig)
    {
        if (_functions.ContainsKey(sig.Name))
        {
            throw new Exception($"Function '{sig.Name}' already declared.");
        }
        if (sig.Name == "start")
        {
            throw new Exception($"Function name '{sig.Name}' not allowed. Reserved label.");
        }
        _functions[sig.Name] = sig;
    }

    public Function Lookup(string name)
    {
        if (!_functions.TryGetValue(name, out Function? sig))
        {
            throw new Exception($"Unknown function: '{name}'");
        }
        return sig;
    }

    public object Clone()
    {
        return new FunctionTable(_functions.ToDictionary(x => x.Key, x => x.Value));
    }
}

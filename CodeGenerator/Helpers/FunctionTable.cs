public class FunctionTable
{
    private readonly Dictionary<string, Function> _functions = new();

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
        if (!_functions.TryGetValue(name, out var sig))
        {
            throw new Exception($"Unknown function: '{name}'");
        }
        return sig;
    }
}

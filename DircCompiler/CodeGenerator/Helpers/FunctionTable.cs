namespace DircCompiler.CodeGen;

public class FunctionTable : ICloneable
{
    private readonly Dictionary<string, Function> _functions = new();
    private readonly CompilerOptions _compilerOptions;
    private readonly CompilerContext _compilerContext;

    public FunctionTable(CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        _compilerOptions = compilerOptions;
        _compilerContext = compilerContext;
    }

    private FunctionTable(CompilerOptions compilerOptions, CompilerContext compilerContext, Dictionary<string, Function> functions)
    {
        _compilerOptions = compilerOptions;
        _compilerContext = compilerContext;
        _functions = functions;
    }

    public void Declare(Function sig, Lexing.Token? identifierToken)
    {
        if (_functions.ContainsKey(sig.Name))
        {
            throw new CodeGenException($"Function '{sig.Name}' already declared.", identifierToken, _compilerOptions, _compilerContext);
        }
        if (sig.Name == "start")
        {
            throw new CodeGenException($"Function name '{sig.Name}' not allowed. Reserved label.", identifierToken, _compilerOptions, _compilerContext);
        }
        _functions[sig.Name] = sig;
    }

    public Function Lookup(string name, Lexing.Token identifierToken)
    {
        if (!_functions.TryGetValue(name, out Function? sig))
        {
            throw new CodeGenException($"Unknown function: '{name}'", identifierToken, _compilerOptions, _compilerContext);
        }
        return sig;
    }

    public object Clone()
    {
        return new FunctionTable(_compilerOptions, _compilerContext, _functions.ToDictionary(x => x.Key, x => x.Value));
    }
}

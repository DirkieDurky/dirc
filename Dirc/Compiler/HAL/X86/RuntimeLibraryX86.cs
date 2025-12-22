using System.Text;
using Dirc.Compiling.Semantic;

namespace Dirc.HAL;

public class RuntimeLibraryX86 : IRuntimeLibrary
{
    private readonly Dictionary<string, FunctionSignature> _functions = new()
    {
    };

    public bool HasFunction(string name)
    {
        return _functions.ContainsKey(name);
    }

    public FunctionSignature GetFunctionSignature(string name)
    {
        FunctionSignature function = _functions[name];
        return function;
    }

    public Dictionary<string, FunctionSignature> GetAllFunctionSignatures()
    {
        Dictionary<string, FunctionSignature> result = [];

        foreach ((string name, FunctionSignature signature) in _functions)
        {
            result.Add(name, signature);
        }

        return result;
    }

    public string GetFunction(string name)
    {
        StringBuilder result = new();

        result.Append($"{name}:");

        StreamReader sr = new StreamReader(Path.Combine(GetPath(), name + ".o"));
        string? line = sr.ReadLine();
        while (line != null)
        {
            result.Append(line);
            line = sr.ReadLine();
        }
        sr.Close();

        result.Append("ret");

        return result.ToString();
    }

    public string GetPath() => Path.Combine(AppContext.BaseDirectory, "lib", GetName());
    public string GetName() => "runtime-x86";
}

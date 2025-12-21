using System.Text;
using Dirc.Compiling.Semantic;

namespace Dirc.HAL;

public class RuntimeLibraryX86 : IRuntimeLibrary
{
    private readonly Dictionary<string, FunctionSignature> _functions = new()
    {
        { "outInt", new FunctionSignature(
            Compiling.Semantic.Void.Instance,
            [
                new FunctionParameter(Int.Instance, "value"),
            ]
        )},
        { "outBool", new FunctionSignature(
            Compiling.Semantic.Void.Instance,
            [
                new FunctionParameter(Bool.Instance, "value")
            ]
        )},
        { "outChar", new FunctionSignature(
            Compiling.Semantic.Void.Instance,
            [
                new FunctionParameter(Compiling.Semantic.Char.Instance, "value")
            ]
        )},
        { "printChar", new FunctionSignature(
            Compiling.Semantic.Void.Instance,
            [
                new FunctionParameter(Compiling.Semantic.Char.Instance, "value")
            ]
        )},
        { "in", new FunctionSignature(
            Int.Instance,
            []
        )},
        { "printNewline", new FunctionSignature(
            Compiling.Semantic.Void.Instance,
            []
        )},
        { "readKey", new FunctionSignature(
            Int.Instance,
            []
        )},
        { "readFileBytes", new FunctionSignature(
            Int.Instance,
            [
                new FunctionParameter(Int.Instance, "fileNum"),
                new FunctionParameter(Int.Instance, "fileOffset")
            ]
        )},
        { "halt2", new FunctionSignature(
            Compiling.Semantic.Void.Instance,
            []
        )},
        { "setScroll", new FunctionSignature(
            Compiling.Semantic.Void.Instance,
            [
                new FunctionParameter(Int.Instance, "offset")
            ]
        )},
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

        result.Append($"label _{name}");

        StreamReader sr = new StreamReader(Path.Combine(GetPath(), name + ".o"));
        string? line = sr.ReadLine();
        while (line != null)
        {
            result.Append(line);
            line = sr.ReadLine();
        }
        sr.Close();

        result.Append("return _ _ _");

        return result.ToString();
    }

    public string GetPath() => Path.Combine(AppContext.BaseDirectory, "lib", GetName());
    public string GetName() => "runtime-diric";
}

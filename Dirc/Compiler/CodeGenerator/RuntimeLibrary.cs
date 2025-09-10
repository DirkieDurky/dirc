using System.Text;
using Dirc.Compiling.Lexing;
using Dirc.Compiling.Parsing;
using Dirc.Compiling.Semantic;

namespace Dirc.Compiling.CodeGen;

static class RuntimeLibrary
{
    private static Token T(string lexeme) => new Token(TokenType.Identifier, lexeme, null, -1);

    private static readonly Dictionary<string, RuntimeFunction> _functions = new()
    {
        { "outInt", new RuntimeFunction("outInt", new FunctionSignature(
            Semantic.Void.Instance, [new FunctionParameter(Int.Instance, "value")]
        ), "outInt.o" )},
        { "outBool", new RuntimeFunction("outBool",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameter(Bool.Instance, "value")]
        ), "outBool.o" )},
        { "outChar", new RuntimeFunction("outChar",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameter(Semantic.Char.Instance, "value")]
        ), "outChar.o" )},
        { "printChar", new RuntimeFunction("printChar",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameter(Semantic.Char.Instance, "value")]
        ), "printChar.o")},
        { "input", new RuntimeFunction("input",
        new FunctionSignature(
            Int.Instance,
            []
        ), "input.o")},
    };

    public static bool HasFunction(string name)
    {
        return _functions.ContainsKey(name);
    }

    public static RuntimeFunction GetFunctionSignature(string name)
    {
        RuntimeFunction function = _functions[name];
        return function;
    }

    public static Dictionary<string, FunctionSignature> GetAllFunctionSignatures()
    {
        Dictionary<string, FunctionSignature> result = [];

        foreach ((string name, RuntimeFunction function) in _functions)
        {
            result.Add(name, function.Signature);
        }

        return result;
    }

    public static string GetFunction(string name)
    {
        StringBuilder result = new();

        RuntimeFunction function = _functions[name];

        result.Append($"label {function.Name}");

        StreamReader sr = new StreamReader(Path.Combine(AppContext.BaseDirectory, "lib", "runtime", function.FilePath));
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
}

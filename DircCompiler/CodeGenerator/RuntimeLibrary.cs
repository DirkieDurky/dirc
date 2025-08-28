using DircCompiler.Lexing;
using DircCompiler.Parsing;
using DircCompiler.Semantic;

namespace DircCompiler.CodeGen;

static class RuntimeLibrary
{
    private static Token T(string lexeme) => new Token(TokenType.Identifier, lexeme, null, -1);

    private static readonly Dictionary<string, RuntimeFunction> _functions = new()
    {
        { "out", new RuntimeFunction("out", new FunctionSignature(
            Semantic.Void.Instance, [new FunctionParameterNode(T("value"), new NamedTypeNode(T("int"), "int"), "value")]
        ), "out.diric" )},
        { "outBool", new RuntimeFunction("outBool",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("bool"), "bool"), "value")]
        ), "outBool.diric" )},
        { "outChar", new RuntimeFunction("outChar",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("char"), "char"), "value")]
        ), "outChar.diric" )},
        { "printChar", new RuntimeFunction("printChar",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("char"), "char"), "value")]
        ), "printChar.diric")},
        { "input", new RuntimeFunction("input",
        new FunctionSignature(
            Int.Instance,
            []
        ), "input.diric")},
        { "malloc", new RuntimeFunction("malloc",
        new FunctionSignature(
            Pointer.Of(Semantic.Void.Instance),
            [new FunctionParameterNode(T("size"), new NamedTypeNode(T("int"), "int"), "size")]
        ),
            // r0 = size
            // r1 = currentAddress
            // r2 = currentValue
            "malloc.diric"
        )},
        { "free", new RuntimeFunction("free",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("ptr"), new PointerTypeNode(T("void*"), new NamedTypeNode(T("void"), "void")), "ptr")]
        ), "free.diric")}
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

    public static void CompileFunction(string name, CodeGenContext context)
    {
        RuntimeFunction function = _functions[name];

        context.CodeGen.EmitLabel(function.Name);

        StreamReader sr = new StreamReader(Path.Combine(AppContext.BaseDirectory, "lib", "runtime", function.FilePath));
        string? line = sr.ReadLine();
        while (line != null)
        {
            line = line.Replace("{{SCREEN_PTR}}", context.ScreenPtr.RegisterEnum.ToString());
            context.CodeGen.Emit(line);
            line = sr.ReadLine();
        }
        sr.Close();

        context.CodeGen.EmitReturn();
    }
}

using DircCompiler.Semantic;

namespace DircCompiler.CodeGen;

static class StandardLibrary
{
    public static Dictionary<string, StandardFunction> Functions = new()
    {
        {"print", new StandardFunction("print", new FunctionSignature("void", [new FunctionParameter("int", "value")]), ["mov r0 _ out"])},
        {"input", new StandardFunction("input", new FunctionSignature("int", []), ["mov in _ r0"])},
    };
}

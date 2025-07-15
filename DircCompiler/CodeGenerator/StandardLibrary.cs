namespace DircCompiler.CodeGen;

class StandardLibrary
{
    public Dictionary<string, StandardFunction> Functions = new()
    {
        {"print", new StandardFunction("print", ["num"], ["mov r0 _ out"])},
    };
}

class StandardLibrary
{
    public Compiler Compiler;
    // public static Delegate Print = Std.PrintMethod;

    // public static string[] PrintMethod(string str)
    // {
    //     return [$"mov _ {str} out"];
    // }

    public StandardLibrary(Compiler compiler)
    {
        Compiler = compiler;
    }

    public void Compile()
    {
        Compiler.CompileStandardFunction("print", ["str"], ["mov _ r0 out"]);
    }
}

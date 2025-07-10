class StandardLibrary
{
    public static string[] PrintMethod(string str)
    {
        return [$"mov _ {str} out"];
    }

    public void Compile(CodeGenContext context)
    {
        context.FuncFactory.CompileStandardFunction(context, "print", ["str"], ["mov _ r0 out"]);
    }
}

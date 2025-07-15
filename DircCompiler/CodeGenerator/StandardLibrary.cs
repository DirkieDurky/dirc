namespace DircCompiler.CodeGen;

class StandardLibrary
{
    public void Compile(CodeGenContext context)
    {
        context.FuncFactory.CompileStandardFunction(context, "print", ["num"], ["mov _ r0 out"]);
    }
}

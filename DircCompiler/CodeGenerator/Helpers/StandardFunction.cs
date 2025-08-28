using DircCompiler.Semantic;

namespace DircCompiler.CodeGen;

public class RuntimeFunction : Symbol
{
    public FunctionSignature Signature;
    public string FilePath;

    public RuntimeFunction(string name, FunctionSignature signature, string filePath) : base(name)
    {
        Signature = signature;
        FilePath = filePath;
    }
}

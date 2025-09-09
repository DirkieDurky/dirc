using Dirc.Compiling.Semantic;

namespace Dirc.Compiling.CodeGen;

public class RuntimeFunction
{
    public string Name;
    public FunctionSignature Signature;
    public string FilePath;

    public RuntimeFunction(string name, FunctionSignature signature, string filePath)
    {
        Name = name;
        Signature = signature;
        FilePath = filePath;
    }
}

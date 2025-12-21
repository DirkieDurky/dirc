using Dirc.Compiling.Semantic;

namespace Dirc.Compiling.CodeGen;

public class RuntimeFunction
{
    public string FilePath;
    public FunctionSignature Signature;

    public RuntimeFunction(FunctionSignature signature, string filePath)
    {
        FilePath = filePath;
        Signature = signature;
    }
}

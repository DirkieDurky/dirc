using DircCompiler.Semantic;

namespace DircCompiler.CodeGen;

public class StandardFunction : Symbol
{
    public FunctionSignature Signature;
    public string[] Code;

    public StandardFunction(string name, FunctionSignature signature, string[] code) : base(name)
    {
        Signature = signature;
        Code = code;
    }
}

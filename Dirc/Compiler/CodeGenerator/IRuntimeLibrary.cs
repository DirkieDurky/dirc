using Dirc.Compiling.Semantic;

namespace Dirc.Compiling.CodeGen;

public interface IRuntimeLibrary
{
    bool HasFunction(string name);
    RuntimeFunction GetFunctionSignature(string name);
    Dictionary<string, FunctionSignature> GetAllFunctionSignatures();
    string GetFunction(string name);
    string GetPath();
    string GetName();
}

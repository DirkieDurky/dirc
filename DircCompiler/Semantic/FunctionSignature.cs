using DircCompiler.Parsing;

namespace DircCompiler.Semantic;

public class FunctionSignature
{
    public Type ReturnType { get; }
    public List<FunctionParameterNode> Parameters { get; }
    public FunctionSignature(Type returnType, List<FunctionParameterNode> parameters)
    {
        ReturnType = returnType;
        Parameters = parameters;
    }
}

namespace DircCompiler.Semantic;

public class FunctionSignature
{
    public Type ReturnType { get; }
    public List<FunctionParameter> Parameters { get; }

    public FunctionSignature(Type returnType, List<FunctionParameter> parameters)
    {
        ReturnType = returnType;
        Parameters = parameters;
    }
}

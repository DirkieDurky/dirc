namespace DircCompiler.Semantic;

public class FunctionSignature
{
    public string ReturnType { get; }
    public List<FunctionParameter> Parameters { get; }
    public FunctionSignature(string returnType, List<FunctionParameter> parameters)
    {
        ReturnType = returnType;
        Parameters = parameters;
    }
}

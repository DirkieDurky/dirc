namespace Dirc.Compiling.Semantic;

public class FunctionSignature
{
    public SimpleType ReturnType { get; }
    public List<FunctionParameter> Parameters { get; }

    public FunctionSignature(SimpleType returnType, List<FunctionParameter> parameters)
    {
        ReturnType = returnType;
        Parameters = parameters;
    }
}

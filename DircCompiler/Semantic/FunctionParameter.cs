namespace DircCompiler.Semantic;

public class FunctionParameter
{
    public Type Type;
    public string Name;

    public FunctionParameter(Type type, string name)
    {
        Type = type;
        Name = name;
    }
}

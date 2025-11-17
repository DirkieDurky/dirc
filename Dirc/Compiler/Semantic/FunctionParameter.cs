namespace Dirc.Compiling.Semantic;

public class FunctionParameter
{
    public SimpleType Type;
    public string Name;

    public FunctionParameter(SimpleType type, string name)
    {
        Type = type;
        Name = name;
    }
}

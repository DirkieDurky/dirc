using DircCompiler.Lexing;

namespace DircCompiler;

public class FunctionParameter
{
    public string TypeName { get; }
    public string Name { get; }
    public FunctionParameter(string typeName, string name)
    {
        TypeName = typeName;
        Name = name;
    }
    public override string ToString() => $"{TypeName} {Name}";
}

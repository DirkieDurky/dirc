namespace DircCompiler.Semantic;

class Pointer : Type
{
    public Type BaseType { get; }
    public Pointer(Type baseType) => BaseType = baseType;

    public override string Name => $"{BaseType.Name}*";
}

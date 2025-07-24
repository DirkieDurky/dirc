namespace DircCompiler.Semantic;

class Pointer : Type
{
    private static readonly Dictionary<Type, Pointer> _cache = new();

    public Type BaseType { get; }

    private Pointer(Type baseType) => BaseType = baseType;

    public static Pointer Of(Type baseType)
    {
        if (!_cache.TryGetValue(baseType, out var pointerType))
        {
            pointerType = new Pointer(baseType);
            _cache[baseType] = pointerType;
        }
        return pointerType;
    }

    public override string Name => $"{BaseType.Name}*";
}

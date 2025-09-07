using System.Collections.Concurrent;

namespace Dirc.Compiling.Semantic;

class Pointer : Type
{
    private static readonly ConcurrentDictionary<Type, Pointer> _cache = new();

    public Type BaseType { get; }

    private Pointer(Type baseType) => BaseType = baseType;

    public static Pointer Of(Type baseType)
    {
        return _cache.GetOrAdd(baseType, e => new Pointer(baseType));
    }

    public override string Name => $"{BaseType.Name}*";
}

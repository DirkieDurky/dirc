using System.Collections.Concurrent;

namespace Dirc.Compiling.Semantic;

class Pointer : SimpleType
{
    private static readonly ConcurrentDictionary<SimpleType, Pointer> _cache = new();

    public SimpleType BaseType { get; }

    private Pointer(SimpleType baseType) => BaseType = baseType;

    public static Pointer Of(SimpleType baseType)
    {
        return _cache.GetOrAdd(baseType, e => new Pointer(baseType));
    }

    public override string Name => $"{BaseType.Name}*";
}

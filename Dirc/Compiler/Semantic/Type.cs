namespace Dirc.Compiling.Semantic;

class Type
{
    public SimpleType SimpleType { get; }
    public List<int?> ArraySizes { get; }

    public Type(SimpleType simpleType, List<int?> arraySizes)
    {
        SimpleType = simpleType;
        ArraySizes = arraySizes;
    }
}

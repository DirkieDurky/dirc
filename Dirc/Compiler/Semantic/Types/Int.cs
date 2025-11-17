namespace Dirc.Compiling.Semantic;

class Int : PrimitiveType
{
    public static SimpleType Instance = new Int();

    public override string Name => "int";

    private Int() { }
}

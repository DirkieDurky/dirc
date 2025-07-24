namespace DircCompiler.Semantic;

class Int : PrimitiveType
{
    public static Type Instance = new Int();

    public override string Name => "int";

    private Int() { }
}

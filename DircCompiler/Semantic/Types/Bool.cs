namespace DircCompiler.Semantic;

class Bool : PrimitiveType
{
    public static Type Instance = new Bool();

    public override string Name => "bool";

    private Bool() { }
}

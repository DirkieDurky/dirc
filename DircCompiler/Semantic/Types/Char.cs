namespace DircCompiler.Semantic;

class Char : PrimitiveType
{
    public static Type Instance = new Char();

    public override string Name => "char";

    private Char() { }
}

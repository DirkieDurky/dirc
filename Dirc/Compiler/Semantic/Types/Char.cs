namespace Dirc.Compiling.Semantic;

class Char : PrimitiveType
{
    public static SimpleType Instance = new Char();

    public override string Name => "char";

    private Char() { }
}

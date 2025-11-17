namespace Dirc.Compiling.Semantic;

class Void : SimpleType
{
    public static SimpleType Instance = new Void();

    public override string Name => "void";

    private Void() { }
}

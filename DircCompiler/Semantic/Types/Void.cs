namespace DircCompiler.Semantic;

class Void : Type
{
    public static Type Instance = new Void();

    public override string Name => "void";

    private Void() { }
}

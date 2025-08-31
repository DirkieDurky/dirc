namespace Dirc.Compiling;

public class CompilerResult
{
    public string Code;
    public string[] Imports;

    public CompilerResult(string code, string[] imports)
    {
        Code = code;
        Imports = imports;
    }
}

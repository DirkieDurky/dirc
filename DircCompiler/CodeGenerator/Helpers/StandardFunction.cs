namespace DircCompiler.CodeGen;

public class StandardFunction : Symbol
{
    public string[] Parameters;
    public string[] Code;

    public StandardFunction(string name, string[] parameters, string[] code) : base(name)
    {
        Parameters = parameters;
        Code = code;
    }
}

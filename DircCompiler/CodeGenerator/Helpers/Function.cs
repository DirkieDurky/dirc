namespace DircCompiler.CodeGen;

public class Function : Symbol
{
    public string[] Parameters;
    public bool Custom;

    public Function(string name, string[] parameters, bool custom) : base(name)
    {
        Parameters = parameters;
        Custom = custom;
    }

    public static Function FromFunctionDeclarationNode(Parsing.FunctionDeclarationNode node)
    {
        return new Function(node.Name, node.Parameters.Select(p => p.Name).ToArray(), true);
    }
}

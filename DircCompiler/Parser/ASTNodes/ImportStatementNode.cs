using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class ImportStatementNode : AstNode
{
    public Token Identifier;
    public string FunctionName;

    public ImportStatementNode(Token identifier, string function)
    {
        Identifier = identifier;
        FunctionName = function;
    }
    public override string ToString() => $"Import({FunctionName})";
}

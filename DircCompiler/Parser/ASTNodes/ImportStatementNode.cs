using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

public class ImportStatementNode : AstNode
{
    public Token Identifier;
    public string LibraryName;

    public ImportStatementNode(Token identifier, string function)
    {
        Identifier = identifier;
        LibraryName = function;
    }
    public override string ToString() => $"Import({LibraryName})";
}

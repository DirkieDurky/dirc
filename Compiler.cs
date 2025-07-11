using System.Diagnostics;

class Compiler
{
    public Compiler()
    {

    }

    public string[] Compile(string source)
    {
        List<Token> tokens = new Lexer().Tokenize(source);

        foreach (Token token in tokens)
        {
            string literal = token.Literal != null ? " " + token.Literal : "";
            Debug.Write($"[{token.Type} {token.Lexeme}{literal}] ");
        }

        List<AstNode> astNodes = new Parser().Parse(tokens);

        foreach (AstNode node in astNodes)
        {
            Console.WriteLine(node);
        }

        string[] assembly = new CodeGenerator().Generate(astNodes);

        return assembly;
    }
}

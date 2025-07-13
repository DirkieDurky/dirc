using Dirc.CodeGen;
using Dirc.Lexing;
using Dirc.Parsing;

namespace Dirc;

class Compiler
{
    public string[] Compile(string source, CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        List<Token> tokens = new Lexer(compilerContext).Tokenize(source);

        if (compilerOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running lexer...");
        }
        if (compilerOptions.ShowLexerOutput)
        {
            foreach (Token token in tokens)
            {
                string literal = token.Literal != null ? " " + token.Literal : "";
                Console.Write($"[{token.Type} {token.Lexeme}{literal}] ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        List<AstNode> astNodes = new Parser(compilerContext).Parse(tokens);

        if (compilerOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running parser...");
        }
        if (compilerOptions.ShowParserOutput)
        {
            foreach (AstNode node in astNodes)
            {
                Console.WriteLine(node);
            }
            Console.WriteLine();
        }

        if (compilerOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running code generator...");
        }
        string[] assembly = new CodeGenerator(compilerOptions, compilerContext).Generate(astNodes);

        return assembly;
    }
}

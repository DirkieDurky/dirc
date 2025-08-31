using Dirc.Compiling.CodeGen;
using Dirc.Compiling.Lexing;
using Dirc.Compiling.Parsing;
using Dirc.Compiling.Semantic;

namespace Dirc.Compiling;

public class Compiler
{
    public CompilerResult Compile(string source, BuildOptions buildOptions, BuildContext buildContext)
    {
        if (buildOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running lexer...");
        }
        List<Token> tokens = new Lexer(buildOptions, buildContext).Tokenize(source);
        if (buildOptions.ShowLexerOutput)
        {
            foreach (Token token in tokens)
            {
                string literal = token.Literal != null ? " " + token.Literal : "";
                Console.Write($"[{token.Type} {token.Lexeme}{literal}] ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        if (buildOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running parser...");
        }
        List<AstNode> astNodes = new Parser(buildOptions, buildContext).Parse(tokens);
        if (buildOptions.ShowParserOutput)
        {
            foreach (AstNode node in astNodes)
            {
                Console.WriteLine(node);
            }
            Console.WriteLine();
        }

        if (buildOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running semantic analyzer...");
        }
        new SemanticAnalyzer(buildOptions, buildContext).Analyze(astNodes, buildOptions, buildContext);
        if (buildOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running code generator...");
        }

        return new CodeGenerator(buildOptions, buildContext).Generate(astNodes);
    }
}

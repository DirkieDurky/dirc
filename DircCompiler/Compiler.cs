using DircCompiler.CodeGen;
using DircCompiler.Lexing;
using DircCompiler.Parsing;
using DircCompiler.Semantic;

namespace DircCompiler;

public class Compiler
{
    public string[] Compile(string source, CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        if (compilerOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running lexer...");
        }
        List<Token> tokens = new Lexer(compilerOptions, compilerContext).Tokenize(source);
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

        if (compilerOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running parser...");
        }
        List<AstNode> astNodes = new Parser(compilerOptions, compilerContext).Parse(tokens);
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
            Console.WriteLine("Running semantic analyzer...");
        }
        new SemanticAnalyzer(compilerOptions, compilerContext).Analyze(astNodes, compilerOptions, compilerContext);
        if (compilerOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running code generator...");
        }
        string[] assembly = new CodeGenerator(compilerOptions, compilerContext).Generate(astNodes);

        return assembly;
    }
}

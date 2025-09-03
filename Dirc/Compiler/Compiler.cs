using Dirc.Compiling.CodeGen;
using Dirc.Compiling.Lexing;
using Dirc.Compiling.Parsing;
using Dirc.Compiling.Semantic;

namespace Dirc.Compiling;

public class Compiler
{
    public FrontEndResult RunFrontEnd(string source, BuildOptions buildOptions, BuildContext buildContext)
    {
        if (buildOptions.ShowGeneralDebug)
        {
            Console.WriteLine($"Running front-end for {buildContext.CurrentFilePath}...");
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
        FrontEndResult frontEndResult = new Parser(buildOptions, buildContext).Parse(tokens);
        if (buildOptions.ShowParserOutput)
        {
            foreach (AstNode node in frontEndResult.AstNodes)
            {
                Console.WriteLine(node);
            }
            Console.WriteLine();
        }

        return frontEndResult;
    }

    public CompilerResult RunBackEnd(List<AstNode> astNodes, SymbolTable symbolTable, BuildOptions buildOptions, BuildContext buildContext)
    {
        if (buildOptions.ShowGeneralDebug)
        {
            Console.WriteLine($"Running back-end for {buildContext.CurrentFilePath}...");
            Console.WriteLine("Running semantic analyzer...");
        }
        new SemanticAnalyzer(buildOptions, buildContext).Analyze(astNodes, symbolTable, buildOptions, buildContext);
        if (buildOptions.ShowGeneralDebug)
        {
            Console.WriteLine("Running code generator...");
        }

        return new CodeGenerator(buildOptions, buildContext).Generate(astNodes);
    }
}

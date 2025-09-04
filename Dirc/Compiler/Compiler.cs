using Dirc.Compiling.CodeGen;
using Dirc.Compiling.Lexing;
using Dirc.Compiling.Parsing;
using Dirc.Compiling.Semantic;

namespace Dirc.Compiling;

public class Compiler
{
    public FrontEndResult RunFrontEnd(string source, Options options, BuildContext buildContext)
    {
        if (options.CheckDebugOption(DebugOption.General))
        {
            Console.WriteLine($"Running front-end for {buildContext.CurrentFilePath}...");
            Console.WriteLine("Running lexer...");
        }
        List<Token> tokens = new Lexer(options, buildContext).Tokenize(source);
        if (options.CheckDebugOption(DebugOption.Lexer))
        {
            foreach (Token token in tokens)
            {
                string literal = token.Literal != null ? " " + token.Literal : "";
                Console.Write($"[{token.Type} {token.Lexeme}{literal}] ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        if (options.CheckDebugOption(DebugOption.General))
        {
            Console.WriteLine("Running parser...");
        }
        FrontEndResult frontEndResult = new Parser(options, buildContext).Parse(tokens);
        if (options.CheckDebugOption(DebugOption.Parser))
        {
            foreach (AstNode node in frontEndResult.AstNodes)
            {
                Console.WriteLine(node);
            }
            Console.WriteLine();
        }

        return frontEndResult;
    }

    public CompilerResult RunBackEnd(List<AstNode> astNodes, SymbolTable symbolTable, Options options, BuildContext buildContext)
    {
        if (options.CheckDebugOption(DebugOption.General))
        {
            Console.WriteLine($"Running back-end for {buildContext.CurrentFilePath}...");
            Console.WriteLine("Running semantic analyzer...");
        }
        new SemanticAnalyzer(options, buildContext).Analyze(astNodes, symbolTable);
        if (options.CheckDebugOption(DebugOption.General))
        {
            Console.WriteLine("Running code generator...");
        }

        return new CodeGenerator(options, buildContext).Generate(astNodes);
    }
}

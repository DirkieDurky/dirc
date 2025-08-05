using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

/// <summary>
/// Main parser class that coordinates the parsing process.
/// </summary>
public class Parser
{
    private readonly ParserBase _parserBase;
    private readonly StatementParser _statementParser;

    public Parser(CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        _parserBase = new ParserBase(compilerOptions, compilerContext);
        _statementParser = new StatementParser(_parserBase);
    }

    public List<AstNode> Parse(List<Token> tokens)
    {
        _parserBase.Initialize(tokens);
        List<AstNode> statements = new();
        
        while (!_parserBase.IsAtEnd())
        {
            statements.AddRange(_statementParser.ParseStatement());
        }
        
        return statements;
    }
}

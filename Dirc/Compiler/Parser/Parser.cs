using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

class Parser
{
    private readonly ParserBase _parserBase;
    private readonly StatementParser _statementParser;

    public Parser(BuildOptions buildOptions, BuildContext buildContext)
    {
        _parserBase = new ParserBase(buildOptions, buildContext);
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

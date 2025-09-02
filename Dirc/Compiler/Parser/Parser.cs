using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

class Parser
{
    private readonly ParserBase _parserBase;
    private readonly ParserContext _context;

    public Parser(BuildOptions buildOptions, BuildContext buildContext)
    {
        _parserBase = new(buildOptions, buildContext);
        _context = new(_parserBase);
    }

    public List<AstNode> Parse(List<Token> tokens)
    {
        _context.ParserBase.Initialize(tokens);
        List<AstNode> statements = new();

        while (!_context.ParserBase.IsAtEnd())
        {
            statements.AddRange(_context.StatementParser.ParseStatement());
        }

        return statements;
    }
}

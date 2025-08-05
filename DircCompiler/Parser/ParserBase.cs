using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

/// <summary>
/// Base class providing common parsing utilities and token management
/// </summary>
internal class ParserBase
{
    private List<Token> _tokens = new();
    private int _current;
    private readonly CompilerContext _context;
    private readonly CompilerOptions _options;

    public ParserBase(CompilerOptions options, CompilerContext context)
    {
        _context = context;
        _options = options;
    }

    public void Initialize(List<Token> tokens)
    {
        _tokens = tokens;
        _current = 0;
    }

    public bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    public bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

    public bool CheckNext(TokenType type) =>
        _current + 1 < _tokens.Count && _tokens[_current + 1].Type == type;

    public Token Advance() => _tokens[_current++];

    public bool IsAtEnd() => _current >= _tokens.Count;

    public Token Peek() => _tokens[_current];

    public Token Previous() => _tokens[_current - 1];

    public void Rewind() => _current--;

    public Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new SyntaxException(message, Previous(), _options, _context);
    }

    public CompilerContext Context => _context;
    public CompilerOptions Options => _options;

    internal VariableAssignmentNode ParseVariableAssignment()
    {
        return new StatementParser(this).ParseVariableAssignment();
    }
}

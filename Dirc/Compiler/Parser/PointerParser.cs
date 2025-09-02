using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of pointer dereference expressions
/// </summary>
internal class PointerParser
{
    private readonly ParserContext _context;

    public PointerParser(ParserContext context)
    {
        _context = context;
    }

    public AstNode? ParsePointerDereference()
    {
        if (!_context.ParserBase.Match(TokenType.Asterisk))
            return null;

        AstNode expr;
        if (_context.ParserBase.Match(TokenType.LeftParen))
        {
            expr = _context.ExpressionParser.ParseExpression();
            _context.ParserBase.Consume(TokenType.RightParen, "Expected ')' after expression");
        }
        else
        {
            Token name = _context.ParserBase.Consume(TokenType.Identifier, "Expected identifier after '*'");
            expr = new IdentifierNode(name, name.Lexeme);
        }
        return new PointerDereferenceNode(expr);
    }
}

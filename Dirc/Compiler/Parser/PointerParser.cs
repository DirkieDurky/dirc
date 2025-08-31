using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of pointer dereference expressions
/// </summary>
internal class PointerParser
{
    private readonly ParserBase _parser;
    private readonly ExpressionParser _expressionParser;

    public PointerParser(ParserBase parser)
    {
        _parser = parser;
        _expressionParser = new ExpressionParser(parser);
    }

    public AstNode? ParsePointerDereference()
    {
        if (!_parser.Match(TokenType.Asterisk))
            return null;

        AstNode expr;
        if (_parser.Match(TokenType.LeftParen))
        {
            expr = _expressionParser.ParseExpression();
            _parser.Consume(TokenType.RightParen, "Expected ')' after expression");
        }
        else
        {
            Token name = _parser.Consume(TokenType.Identifier, "Expected identifier after '*'");
            expr = new IdentifierNode(name, name.Lexeme);
        }
        return new PointerDereferenceNode(expr);
    }
}

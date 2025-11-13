using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of expressions including binary operations, literals, and primary expressions
/// </summary>
class ExpressionParser
{
    private readonly ParserContext _context;

    public ExpressionParser(ParserContext context)
    {
        _context = context;
    }

    public AstNode ParseExpression()
    {
        return ParseOperatorPrecedence(0); // Start at lowest precedence level
    }

    private AstNode ParseOperatorPrecedence(int precedenceLevel)
    {
        if (precedenceLevel >= _context.OperatorPrecedence.MaxLevel)
        {
            return ParsePrimary();
        }

        AstNode left = ParseOperatorPrecedence(precedenceLevel + 1);

        while (_context.OperatorPrecedence.IsOperatorAtLevel(precedenceLevel, _context.ParserBase.Peek().Type))
        {
            Token operatorToken = _context.ParserBase.Advance();
            AstNode right = ParseOperatorPrecedence(precedenceLevel + 1);
            Operation op = _context.OperatorPrecedence.GetOperation(operatorToken.Type);
            left = new BinaryExpressionNode(op, left, right);
        }

        return left;
    }

    private AstNode ParsePrimary()
    {
        if (_context.ParserBase.IsAtEnd())
            throw new SyntaxException("Unexpected end of text", _context.ParserBase.Previous(), _context.ParserBase.Options, _context.ParserBase.Context);

        if (_context.ParserBase.Match(TokenType.LeftParen))
        {
            AstNode expr = ParseExpression();
            _context.ParserBase.Consume(TokenType.RightParen, "Expected ')' after expression");
            return expr;
        }

        if (_context.ParserBase.Match(TokenType.Asterisk))
            return ParsePointerDereference();

        if (_context.ParserBase.Match(TokenType.Ampersand))
        {
            Token name = _context.ParserBase.Consume(TokenType.Identifier, "Expected identifier after '&'");
            return new AddressOfNode(new IdentifierNode(name, name.Lexeme));
        }

        if (_context.ParserBase.Match(TokenType.Number))
            return new NumberLiteralNode(NumberLiteralType.Decimal, (string)_context.ParserBase.Previous().Literal!);

        if (_context.ParserBase.Match(TokenType.BinaryNumber))
            return new NumberLiteralNode(NumberLiteralType.Binary, (string)_context.ParserBase.Previous().Literal!);

        if (_context.ParserBase.Match(TokenType.HexNumber))
            return new NumberLiteralNode(NumberLiteralType.Hexadecimal, (string)_context.ParserBase.Previous().Literal!);

        if (_context.ParserBase.Match(TokenType.True))
            return new BooleanLiteralNode(true);

        if (_context.ParserBase.Match(TokenType.False))
            return new BooleanLiteralNode(false);

        if (_context.ParserBase.Match(TokenType.Identifier))
            return ParseIdentifierExpression();

        if (_context.ParserBase.Match(TokenType.Char))
        {
            return new CharNode((char)_context.ParserBase.Previous().Literal!);
        }

        if (_context.ParserBase.Match(TokenType.String))
        {
            return new StringLiteralNode(_context.ParserBase.Previous());
        }

        if (_context.ParserBase.Check(TokenType.LeftBrace))
        {
            return _context.ArrayParser.ParseArrayLiteral();
        }

        throw new SyntaxException("Unexpected token", _context.ParserBase.Peek(), _context.ParserBase.Options, _context.ParserBase.Context);
    }

    private AstNode ParsePointerDereference()
    {
        AstNode expr;
        if (_context.ParserBase.Match(TokenType.LeftParen))
        {
            expr = ParseExpression();
            _context.ParserBase.Consume(TokenType.RightParen, "Expected ')' after expression");
        }
        else
        {
            Token name = _context.ParserBase.Consume(TokenType.Identifier, "Expected identifier after '*'");
            expr = new IdentifierNode(name, name.Lexeme);
        }
        return new PointerDereferenceNode(expr);
    }

    private AstNode ParseIdentifierExpression()
    {
        Token name = _context.ParserBase.Previous();

        if (_context.ParserBase.Match(TokenType.LeftParen))
            return ParseFunctionCall(name);

        if (_context.ParserBase.Match(TokenType.LeftBracket))
            return ParseArrayOperation(name);

        if (_context.ParserBase.Check(TokenType.Equals) ||
            (_context.OperatorPrecedence.IsValidOperation(_context.ParserBase.Peek().Type) && _context.ParserBase.CheckNext(TokenType.Equals)) ||
            (_context.ParserBase.Check(TokenType.Plus) && _context.ParserBase.CheckNext(TokenType.Plus)) ||
            (_context.ParserBase.Check(TokenType.Minus) && _context.ParserBase.CheckNext(TokenType.Minus)))
        {
            _context.ParserBase.Rewind();
            return _context.VariableParser.ParseVariableAssignment();
        }

        return new IdentifierNode(_context.ParserBase.Previous(), _context.ParserBase.Previous().Lexeme);
    }

    private AstNode ParseArrayOperation(Token name)
    {
        AstNode index = ParseExpression();
        _context.ParserBase.Consume(TokenType.RightBracket, "Expected ']' after array index");

        if (_context.ParserBase.Match(TokenType.Equals))
        {
            AstNode value = ParseExpression();
            return new ArrayAssignmentNode(name, index, value);
        }

        return new ArrayAccessNode(name, index);
    }

    internal AstNode ParseFunctionCall(Token name)
    {
        List<AstNode> args = new();
        if (!_context.ParserBase.Check(TokenType.RightParen))
        {
            do
            {
                args.Add(ParseExpression());
            } while (_context.ParserBase.Match(TokenType.Comma));
        }
        _context.ParserBase.Consume(TokenType.RightParen, "Expected ')' after arguments");
        return new CallExpressionNode(name, name.Lexeme, args);
    }
}

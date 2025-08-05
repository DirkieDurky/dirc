using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

/// <summary>
/// Handles parsing of expressions including binary operations, literals, and primary expressions
/// </summary>
class ExpressionParser
{
    private readonly ParserBase _parser;
    private readonly OperatorPrecedence _precedence;

    public ExpressionParser(ParserBase parser)
    {
        _parser = parser;
        _precedence = new OperatorPrecedence();
    }

    public AstNode ParseExpression()
    {
        return ParseOperatorPrecedence(0); // Start at lowest precedence level
    }

    private AstNode ParseOperatorPrecedence(int precedenceLevel)
    {
        if (precedenceLevel >= _precedence.MaxLevel)
        {
            return ParsePrimary();
        }

        AstNode left = ParseOperatorPrecedence(precedenceLevel + 1);

        while (_precedence.IsOperatorAtLevel(precedenceLevel, _parser.Peek().Type))
        {
            Token operatorToken = _parser.Advance();
            AstNode right = ParseOperatorPrecedence(precedenceLevel + 1);
            Operation op = _precedence.GetOperation(operatorToken.Type);
            left = new BinaryExpressionNode(op, left, right);
        }

        return left;
    }

    private AstNode ParsePrimary()
    {
        if (_parser.IsAtEnd())
            throw new SyntaxException("Unexpected end of text", _parser.Previous(), _parser.Options, _parser.Context);

        AstNode? pointerDereference = ParsePointerDereference();
        if (pointerDereference != null) return pointerDereference;

        if (_parser.Match(TokenType.Ampersand))
        {
            Token name = _parser.Consume(TokenType.Identifier, "Expected identifier after '&'");
            return new AddressOfNode(new IdentifierNode(name, name.Lexeme));
        }

        if (_parser.Match(TokenType.Number))
            return new NumberLiteralNode(NumberLiteralType.Decimal, (string)_parser.Previous().Literal!);

        if (_parser.Match(TokenType.BinaryNumber))
            return new NumberLiteralNode(NumberLiteralType.Binary, (string)_parser.Previous().Literal!);

        if (_parser.Match(TokenType.HexNumber))
            return new NumberLiteralNode(NumberLiteralType.Hexadecimal, (string)_parser.Previous().Literal!);

        if (_parser.Match(TokenType.True))
            return new BooleanLiteralNode(true);

        if (_parser.Match(TokenType.False))
            return new BooleanLiteralNode(false);

        if (_parser.Match(TokenType.Identifier))
            return ParseIdentifierExpression();

        throw new SyntaxException("Unexpected token", _parser.Peek(), _parser.Options, _parser.Context);
    }

    private AstNode? ParsePointerDereference()
    {
        if (!_parser.Match(TokenType.Asterisk)) return null;

        AstNode expr;
        if (_parser.Match(TokenType.LeftParen))
        {
            expr = ParseExpression();
            _parser.Consume(TokenType.RightParen, "Expected ')' after expression");
        }
        else
        {
            Token name = _parser.Consume(TokenType.Identifier, "Expected identifier after '*'");
            expr = new IdentifierNode(name, name.Lexeme);
        }
        return new PointerDereferenceNode(expr);
    }

    private AstNode ParseIdentifierExpression()
    {
        Token name = _parser.Previous();

        if (_parser.Match(TokenType.LeftParen))
            return ParseFunctionCall(name);

        if (_parser.Match(TokenType.LeftBracket))
            return ParseArrayOperation(name);

        if (_parser.Check(TokenType.Equals) ||
            (_precedence.IsValidOperation(_parser.Peek().Type) && _parser.CheckNext(TokenType.Equals)) ||
            (_parser.Check(TokenType.Plus) && _parser.CheckNext(TokenType.Plus)) ||
            (_parser.Check(TokenType.Minus) && _parser.CheckNext(TokenType.Minus)))
        {
            _parser.Rewind();
            return _parser.ParseVariableAssignment();
        }

        return new IdentifierNode(_parser.Previous(), _parser.Previous().Lexeme);
    }

    private AstNode ParseArrayOperation(Token name)
    {
        AstNode index = ParseExpression();
        _parser.Consume(TokenType.RightBracket, "Expected ']' after array index");

        if (_parser.Match(TokenType.Equals))
        {
            AstNode value = ParseExpression();
            return new ArrayAssignmentNode(name, index, value);
        }

        return new ArrayAccessNode(name, index);
    }

    internal AstNode ParseFunctionCall(Token name)
    {
        List<AstNode> args = new();
        if (!_parser.Check(TokenType.RightParen))
        {
            do
            {
                args.Add(ParseExpression());
            } while (_parser.Match(TokenType.Comma));
        }
        _parser.Consume(TokenType.RightParen, "Expected ')' after arguments");
        return new CallExpressionNode(name, name.Lexeme, args);
    }
}

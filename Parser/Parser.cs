class Parser
{
    private List<Token> _tokens = new();
    private int _current;

    public List<AstNode> Parse(List<Token> tokens)
    {
        _tokens = tokens;
        _current = 0;
        var statements = new List<AstNode>();
        while (!IsAtEnd())
        {
            statements.Add(ParseStatement());
        }
        return statements;
    }

    private AstNode ParseStatement()
    {
        if (Match(TokenType.Function))
            return ParseFunctionDeclaration();
        return ParseExpressionStatement();
    }

    private FunctionDeclarationNode ParseFunctionDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected function name.").Lexeme;
        Consume(TokenType.LeftParen, "Expected '(' after function name.");
        var parameters = new List<string>();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name.").Lexeme);
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expected ')' after parameters.");
        Consume(TokenType.LeftBrace, "Expected '{' before function body.");
        var body = new List<AstNode>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            body.Add(ParseStatement());
        }
        Consume(TokenType.RightBrace, "Expected '}' after function body.");
        return new FunctionDeclarationNode(name, parameters, body);
    }

    private ExpressionStatementNode ParseExpressionStatement()
    {
        var expr = ParseExpression();
        Consume(TokenType.Semicolon, "Expected ';' after expression.");
        return new ExpressionStatementNode(expr);
    }

    private AstNode ParseExpression()
    {
        return ParseOr();
    }

    // Operator precedence: Or | Xor | And | Addition | Multiplication | Primary
    private AstNode ParseOr()
    {
        var expr = ParseXor();
        while (Match(TokenType.Or))
        {
            var op = Previous().Lexeme;
            var right = ParseXor();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParseXor()
    {
        var expr = ParseAnd();
        while (Match(TokenType.Xor))
        {
            var op = Previous().Lexeme;
            var right = ParseAnd();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParseAnd()
    {
        var expr = ParseAddition();
        while (Match(TokenType.And))
        {
            var op = Previous().Lexeme;
            var right = ParseAddition();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParseAddition()
    {
        var expr = ParseMultiplication();
        while (Match(TokenType.Plus) || Match(TokenType.Minus))
        {
            var op = Previous().Lexeme;
            var right = ParseMultiplication();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParseMultiplication()
    {
        var expr = ParsePrimary();
        while (Match(TokenType.Asterisk) || Match(TokenType.Slash))
        {
            var op = Previous().Lexeme;
            var right = ParsePrimary();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParsePrimary()
    {
        if (Match(TokenType.Number))
        {
            return new NumberLiteralNode((int)Previous().Literal!);
        }

        if (Match(TokenType.Identifier))
        {
            var name = Previous().Lexeme;
            if (Match(TokenType.LeftParen))
            {
                var args = new List<AstNode>();
                if (!Check(TokenType.RightParen))
                {
                    do
                    {
                        args.Add(ParseExpression());
                    } while (Match(TokenType.Comma));
                }
                Consume(TokenType.RightParen, "Expected ')' after arguments.");
                return new CallExpressionNode(name, args);
            }
            return new IdentifierNode(name);
        }
        throw new Exception("Unexpected token: " + Peek().Lexeme);
    }

    // Utility methods
    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        return _tokens[_current++];
    }

    private bool IsAtEnd() => _current >= _tokens.Count;
    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new Exception(message);
    }
}
